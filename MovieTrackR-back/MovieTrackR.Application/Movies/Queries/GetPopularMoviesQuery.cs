using AutoMapper;
using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetPopularMoviesQuery(MovieSearchCriteria searchCriteria)
    : IRequest<HybridPagedResult<SearchMovieResultDto>>;

public sealed class GetPopularMoviesHandler(ITmdbClientService tmdbClient, IMapper mapper)
    : IRequestHandler<GetPopularMoviesQuery, HybridPagedResult<SearchMovieResultDto>>
{
    public async Task<HybridPagedResult<SearchMovieResultDto>> Handle(GetPopularMoviesQuery searchQuery, CancellationToken cancellationToken)
    {
        MovieSearchCriteria searchCriteria = searchQuery.searchCriteria;

        TmdbSearchMoviesResponse tmdb = await tmdbClient.GetPopularMovies(
            language: "fr-FR",
            page: searchCriteria.Page,
            cancellationToken: cancellationToken);

        int totalTmdb = tmdb.TotalResults;
        int? totalTmdbPages = tmdb.TotalPages;
        List<SearchMovieResultDto> tmdbDtos = mapper.Map<List<SearchMovieResultDto>>(tmdb.Results);

        bool hasMore = totalTmdbPages is int tp && searchCriteria.Page < tp;

        PageMeta meta = new PageMeta(
        Page: searchCriteria.Page,
        PageSize: searchCriteria.PageSize,
        TotalLocal: 0,
        TotalTmdb: totalTmdb,
        TotalResults: totalTmdb,
        TotalTmdbPages: totalTmdbPages,
        HasMore: hasMore
        );

        return new HybridPagedResult<SearchMovieResultDto>(tmdbDtos, meta);
    }
}