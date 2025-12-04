using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.People.Queries;

public sealed record SearchPeopleQuery(PeopleSearchCriteria searchCriteria)
    : IRequest<HybridPagedResult<SearchPersonResultDto>>;

public sealed class SearchPeopleHandler(IMovieTrackRDbContext dbContext, ITmdbClient tmdbClient, IMapper mapper)
    : IRequestHandler<SearchPeopleQuery, HybridPagedResult<SearchPersonResultDto>>
{
    public async Task<HybridPagedResult<SearchPersonResultDto>> Handle(SearchPeopleQuery searchQuery, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.searchCriteria.Query))
        {
            PageMeta emptyMeta = new PageMeta(
                Page: searchQuery.searchCriteria.Page,
                PageSize: searchQuery.searchCriteria.PageSize,
                TotalLocal: 0,
                TotalTmdb: 0,
                TotalResults: 0,
                TotalTmdbPages: null,
                HasMore: false
            );
            return new HybridPagedResult<SearchPersonResultDto>([], emptyMeta);
        }

        int skip = (searchQuery.searchCriteria.Page - 1) * searchQuery.searchCriteria.PageSize;
        int take = searchQuery.searchCriteria.PageSize;
        string searchTerm = searchQuery.searchCriteria.Query.Trim().ToLower();

        // 1. Recherche locale
        IQueryable<Person> localQuery = dbContext.People
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(searchTerm))
            .OrderBy(p => p.Name);

        int totalLocal = await localQuery.CountAsync(cancellationToken);

        List<SearchPersonResultDto> localPeople = await localQuery
            .Skip(skip)
            .Take(take)
            .ProjectTo<SearchPersonResultDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        localPeople.ForEach(p => p.IsLocal = true);

        // 2. Recherche TMDB
        int totalTmdb = 0;
        int? totalTmdbPages = null;
        List<SearchPersonResultDto> tmdbPeople = [];

        try
        {
            TmdbSearchPeopleResponse tmdbResult = await tmdbClient.SearchPeopleAsync(
                query: searchQuery.searchCriteria.Query,
                page: searchQuery.searchCriteria.Page,
                language: "fr-FR",
                cancellationToken: cancellationToken);

            totalTmdb = tmdbResult.TotalResults;
            totalTmdbPages = tmdbResult.TotalPages;
            tmdbPeople = mapper.Map<List<SearchPersonResultDto>>(tmdbResult.Results);
            tmdbPeople.ForEach(p => p.IsLocal = false);
        }
        catch
        {
        }

        // 3. Fusionner les résultats locaux et TMDB en évitant les doublons
        HashSet<string> localKeys = new HashSet<string>(
            localPeople.Select(MakeKey),
            StringComparer.OrdinalIgnoreCase);

        List<SearchPersonResultDto> merged = localPeople.Concat(
                tmdbPeople.Where(x => !localKeys.Contains(MakeKey(x)))
            )
            .Take(searchQuery.searchCriteria.PageSize)
            .ToList();

        bool localHasMore = totalLocal > (skip + take);
        bool tmdbHasMore = totalTmdbPages is int tp && searchQuery.searchCriteria.Page < tp;
        bool hasMore = localHasMore || tmdbHasMore;

        PageMeta meta = new PageMeta(
            Page: searchQuery.searchCriteria.Page,
            PageSize: searchQuery.searchCriteria.PageSize,
            TotalLocal: totalLocal,
            TotalTmdb: totalTmdb,
            TotalResults: totalLocal + totalTmdb,
            TotalTmdbPages: totalTmdbPages,
            HasMore: hasMore
        );

        return new HybridPagedResult<SearchPersonResultDto>(merged, meta);
    }

    static string MakeKey(SearchPersonResultDto x)
    {
        if (x.TmdbId is int id) return $"tmdb:{id}";
        var t = (x.Name ?? "").Trim();
        var y = x.ProfilePath ?? "";
        return $"t:{t}|y:{y}";
    }
}