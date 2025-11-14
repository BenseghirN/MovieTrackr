using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Helpers;
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
            .AsNoTracking();

        localQuery = localQuery.ApplyTitleFilter((DbContext)dbContext, searchCriteria.Query);

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
        int totalTmdb = 0;
        int? totalTmdbPages = null;
        List<SearchMovieResultDto> tmdbDtos = [];

        if (!string.IsNullOrWhiteSpace(searchCriteria.Query))
        {
            TmdbSearchMoviesResponse tmdb = await tmdbClient.SearchMoviesAsync(
                query: searchCriteria.Query!, page: searchCriteria.Page, language: "fr-FR", region: null, cancellationToken: cancellationToken);

            totalTmdb = tmdb.TotalResults;
            totalTmdbPages = tmdb.TotalPages;
            tmdbDtos = mapper.Map<List<SearchMovieResultDto>>(tmdb.Results);
        }

        // 3. Fusionner les résultats locaux et TMDB en évitant les doublons
        HashSet<string> localKeys =
            new HashSet<string>(locals.Select(MakeKey), StringComparer.OrdinalIgnoreCase);

        List<SearchMovieResultDto> merged =
            locals.Concat(
                tmdbDtos.Where(x => !localKeys.Contains(MakeKey(x)))
            )
            .Take(searchCriteria.PageSize)
            .ToList();

        bool localHasMore = totalLocal > (Skip + Take);
        bool tmdbHasMore = totalTmdbPages is int tp && searchCriteria.Page < tp;
        bool hasMore = localHasMore || tmdbHasMore;

        PageMeta meta = new PageMeta(
        Page: searchCriteria.Page,
        PageSize: searchCriteria.PageSize,
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