using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record SearchMoviesHybridQuery(MovieSearchCriteria searchCriteria)
    : IRequest<HybridPagedResult<SearchMovieResultDto>>;

public sealed class SearchMoviesHybridHandler(IMovieTrackRDbContext dbContext, ITmdbClient tmdbClient, IMapper mapper)
    : IRequestHandler<SearchMoviesHybridQuery, HybridPagedResult<SearchMovieResultDto>>
{
    public async Task<HybridPagedResult<SearchMovieResultDto>> Handle(SearchMoviesHybridQuery searchQuery, CancellationToken cancellationToken)
    {
        MovieSearchCriteria searchCriteria = searchQuery.searchCriteria;
        var (Skip, Take) = searchCriteria.Paging();

        // 1. Requête locale
        IQueryable<Movie> localQuery = dbContext.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchCriteria.Query))
            localQuery = localQuery.Where(m =>
                EF.Functions.ILike(m.Title, $"%{searchCriteria.Query}%") ||
                (m.OriginalTitle != null && EF.Functions.ILike(m.OriginalTitle, $"%{searchCriteria.Query}%")));

        if (searchCriteria.Year is not null) localQuery = localQuery.Where(m => m.Year == searchCriteria.Year);

        if (searchCriteria.GenreId is not null)
            localQuery = localQuery.Where(m => m.MovieGenres.Any(mg => mg.GenreId == searchCriteria.GenreId));

        localQuery = searchCriteria.Sort?.ToLowerInvariant() switch
        {
            "year" => localQuery.OrderByDescending(m => m.Year).ThenBy(m => m.Title),
            "title" => localQuery.OrderBy(m => m.Title),
            _ => localQuery.OrderBy(m => m.Title)
        };

        int totalLocal = await localQuery.CountAsync(cancellationToken);

        List<SearchMovieResultDto> locals = await localQuery
            .Skip(Skip).Take(Take)
            .ProjectTo<SearchMovieResultDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // 2. Requête TMDB
        int? tmdbTotalResults = null;
        int? tmdbTotalPages = null;
        List<SearchMovieResultDto> tmdbDtos = [];

        if (!string.IsNullOrWhiteSpace(searchCriteria.Query))
        {
            // TmdbSearchMoviesResponse tmdb = await tmdbClient.SearchMoviesAsync(searchCriteria.Search!, searchCriteria.Page, language: null!, region: null, cancellationToken);
            TmdbSearchMoviesResponse tmdb = await tmdbClient.SearchMoviesAsync(
                query: searchCriteria.Query!, page: searchCriteria.Page, language: "fr-FR", region: null, cancellationToken: cancellationToken);
            tmdbTotalResults = tmdb.TotalResults;
            tmdbTotalPages = tmdb.TotalPages;
            tmdbDtos = mapper.Map<List<SearchMovieResultDto>>(tmdb.Results);
        }

        // 3. Fusionner les résultats locaux et TMDB en évitant les doublons
        HashSet<string> localKeys = new HashSet<string>(
            locals.Select(x => $"{x.TmdbId?.ToString() ?? ""}|{x.Title}|{x.Year}"),
            StringComparer.OrdinalIgnoreCase);

        List<SearchMovieResultDto> merged =
            locals.Concat(
                tmdbDtos.Where(x =>
                    !localKeys.Contains($"{x.TmdbId?.ToString() ?? ""}|{x.Title}|{x.Year}"))
            )
            .Take(searchCriteria.PageSize)
            .ToList();

        bool hasMore =
            (tmdbTotalPages is int tp && searchCriteria.Page < tp) ||
            (totalLocal > (Skip + Take));

        var meta = new PageMeta(
            Page: searchCriteria.Page,
            PageSize: searchCriteria.PageSize,
            TotalPages: tmdbTotalPages,
            TotalResults: tmdbTotalResults,
            TotalLocal: totalLocal,
            HasMore: hasMore
        );

        return new HybridPagedResult<SearchMovieResultDto>(merged, meta);
    }
}