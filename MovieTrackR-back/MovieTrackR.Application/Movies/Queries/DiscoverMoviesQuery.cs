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

public sealed record DiscoverMoviesQuery(DiscoverCriteria Criterias) : IRequest<HybridPagedResult<SearchMovieResultDto>>;

public sealed class DiscoverMoviesHandler(IMovieTrackRDbContext dbContext, ITmdbClientService tmdbClient, IMapper mapper)
    : IRequestHandler<DiscoverMoviesQuery, HybridPagedResult<SearchMovieResultDto>>
{
    public async Task<HybridPagedResult<SearchMovieResultDto>> Handle(DiscoverMoviesQuery query, CancellationToken cancellationToken)
    {
        DiscoverCriteria criterias = query.Criterias;

        if (criterias.Year is null && (criterias.GenreIds is null || criterias.GenreIds.Count == 0))
            throw new ArgumentException("Discover requires at least a year or genre.");

        var (Skip, Take) = criterias.Paging();

        // 1. Requête locale
        List<SearchMovieResultDto> localResults = [];
        IQueryable<Movie> localQuery = dbContext.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .AsNoTracking();

        if (criterias.Year.HasValue) localQuery = localQuery.Where(m => m.Year == criterias.Year);

        if (criterias.GenreIds?.Count > 0)
            localQuery = localQuery.Where(m => m.MovieGenres.Any(mg => mg.Genre.TmdbId.HasValue && criterias.GenreIds.Contains(mg.Genre.TmdbId.Value)));

        localQuery = localQuery.OrderBy(m => m.Title)
                .ThenBy(m => m.Year)
                .ThenBy(m => m.TmdbId);

        int totalLocal = await localQuery.CountAsync(cancellationToken);

        localResults = await localQuery
            .Skip(Skip).Take(Take)
            .ProjectTo<SearchMovieResultDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // 2. Requête TMDB
        List<SearchMovieResultDto> tmdbResults = [];
        int totalTmdb = 0;
        int? totalTmdbPages = null;

        TmdbSearchMoviesResponse tmdb = await tmdbClient.DiscoverAsync(
                criterias: criterias,
                cancellationToken: cancellationToken);

        totalTmdb = tmdb.TotalResults;
        totalTmdbPages = tmdb.TotalPages;
        tmdbResults = mapper.Map<List<SearchMovieResultDto>>(tmdb.Results);

        // 3.Fusionner les résultats locaux et TMDB en évitant les doublons
        HashSet<string> localKeys = new HashSet<string>(
            localResults.Select(MakeKey),
            StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<SearchMovieResultDto> merged = localResults.Concat(
                tmdbResults.Where(movie => !localKeys.Contains(MakeKey(movie)))
            )
            // .Take(criterias.PageSize)
            .ToList();

        bool localHasMore = totalLocal > (Skip + Take);
        bool tmdbHasMore = totalTmdbPages is int tp && criterias.Page < tp;
        bool hasMore = localHasMore || tmdbHasMore;

        PageMeta meta = new PageMeta(
        Page: criterias.Page,
        PageSize: criterias.PageSize,
        TotalLocal: totalLocal,
        TotalTmdb: totalTmdb,
        TotalResults: totalLocal + totalTmdb,
        TotalTmdbPages: totalTmdbPages,
        HasMore: hasMore
        );

        return new HybridPagedResult<SearchMovieResultDto>(merged, meta);
    }

    static string MakeKey(SearchMovieResultDto x)
    {
        if (x.TmdbId is int id) return $"tmdb:{id}";
        var t = (x.Title ?? "").Trim();
        var y = x.Year?.ToString() ?? "";
        return $"t:{t}|y:{y}";
    }
}