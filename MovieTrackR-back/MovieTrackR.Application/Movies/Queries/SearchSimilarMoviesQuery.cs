using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record SearchSimilarMoviesQuery(int? TmdbMovieId) : IRequest<IReadOnlyList<SearchMovieResultDto>>;

public sealed class SearchSimilarMovieHandler(IMovieTrackRDbContext dbContext, ITmdbClientService tmdbClient, IMapper mapper)
    : IRequestHandler<SearchSimilarMoviesQuery, IReadOnlyList<SearchMovieResultDto>>
{
    public async Task<IReadOnlyList<SearchMovieResultDto>> Handle(SearchSimilarMoviesQuery searchQuery, CancellationToken cancellationToken)
    {
        if (searchQuery.TmdbMovieId is null)
            throw new ArgumentException("Provide either LocalMovieId or TmdbMovieId");

        // 1. Requête locale
        List<SearchMovieResultDto> localResults = [];
        var sourceMovie = await GetSourceMovieAsync(searchQuery.TmdbMovieId, cancellationToken);
        if (sourceMovie.HasValue)
            localResults = await GetLocalSimilarMoviesAsync(sourceMovie.Value.LocalId, cancellationToken);

        // 2. Requête TMDB
        List<SearchMovieResultDto> tmdbResults = [];

        if (searchQuery.TmdbMovieId.HasValue)
        {
            TmdbSearchMoviesResponse tmdb = await tmdbClient.SearchSimilarMoviesAsync(
                tmdbMovieId: searchQuery.TmdbMovieId.Value,
                language: "fr-FR",
                cancellationToken: cancellationToken);

            tmdbResults = mapper.Map<List<SearchMovieResultDto>>(tmdb.Results);
        }

        // 3. Fusionner les résultats locaux et TMDB en évitant les doublons
        HashSet<string> localKeys = new HashSet<string>(
            localResults.Select(MakeKey),
            StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<SearchMovieResultDto> merged = localResults.Concat(
                tmdbResults.Where(movie => !localKeys.Contains(MakeKey(movie)))
            )
            .Take(10)
            .ToList();

        return merged;
    }

    private async Task<(Guid LocalId, int? TmdbId)?> GetSourceMovieAsync(int? tmdbId, CancellationToken cancellationToken)
    {
        if (!tmdbId.HasValue)
            return null;

        var movie = await dbContext.Movies
            .AsNoTracking()
            .Where(m => m.TmdbId == tmdbId.Value)
            .Select(m => new { m.Id, m.TmdbId })
            .FirstOrDefaultAsync(cancellationToken);

        return movie is null ? null : (movie.Id, movie.TmdbId);
    }

    private async Task<List<SearchMovieResultDto>> GetLocalSimilarMoviesAsync(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        // Récupère les critères du film source
        var sourceMovie = await dbContext.Movies
            .AsNoTracking()
            .Where(m => m.Id == movieId)
            .Select(m => new
            {
                m.Id,
                m.ReleaseDate,
                // Utilise les navigation properties de ton entité
                GenreIds = m.MovieGenres.Select(mg => mg.GenreId).ToList(),
                // Cast = collection via ta navigation property
                ActorIds = m.Cast
                    .OrderBy(mc => mc.Order)
                    .Take(5) // Top 5 acteurs principaux
                    .Select(mc => mc.PersonId)
                    .ToList(),
                // Crew = collection via ta navigation property
                DirectorIds = m.Crew
                    .Where(mc => mc.Job == "Director")
                    .Select(mc => mc.PersonId)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sourceMovie is null)
            return new List<SearchMovieResultDto>();

        // Recherche avec scoring
        var results = await dbContext.Movies
            .AsNoTracking()
            .AsSplitQuery()
            .Where(m => m.Id != movieId && m.ReleaseDate != null)
            .Select(m => new
            {
                Movie = m,
                // 🎯 Scoring pondéré (utilise Cast et Crew)
                GenreMatches = m.MovieGenres.Count(mg => sourceMovie.GenreIds.Contains(mg.GenreId)),
                ActorMatches = m.Cast.Count(mc => sourceMovie.ActorIds.Contains(mc.PersonId)),
                DirectorMatches = m.Crew.Count(mc =>
                    sourceMovie.DirectorIds.Contains(mc.PersonId) && mc.Job == "Director"),
                // Proximité temporelle (films de la même époque)
                YearDiff = sourceMovie.ReleaseDate.HasValue && m.ReleaseDate.HasValue
                    ? Math.Abs(sourceMovie.ReleaseDate.Value.Year - m.ReleaseDate.Value.Year)
                    : 999
            })
            .Select(x => new
            {
                x.Movie,
                x.GenreMatches,
                x.ActorMatches,
                x.DirectorMatches,
                x.YearDiff,
                // Score total pondéré
                Score = (x.GenreMatches * 3.0) +         // Genres = important
                        (x.ActorMatches * 2.5) +         // Acteurs = très important
                        (x.DirectorMatches * 5.0) +      // Réalisateur = crucial
                        (x.YearDiff < 5 ? 2.0 : 0) +     // Même époque = bonus
                        ((x.Movie.VoteAverage ?? 0) / 2.0) // Popularité = tie-breaker
            })
            .Where(x => x.Score > 3.0) // Au moins 1 genre ou 1 acteur en commun
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Movie.VoteAverage)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Mapping vers DTO en utilisant AutoMapper
        return results.Select(x =>
        {
            SearchMovieResultDto dto = mapper.Map<SearchMovieResultDto>(x.Movie);
            dto.IsLocal = true;
            return dto;
        }).ToList();
    }

    static string MakeKey(SearchMovieResultDto x)
    {
        if (x.TmdbId is int id) return $"tmdb:{id}";
        var t = (x.Title ?? "").Trim();
        var y = x.Year?.ToString() ?? "";
        return $"t:{t}|y:{y}";
    }
}