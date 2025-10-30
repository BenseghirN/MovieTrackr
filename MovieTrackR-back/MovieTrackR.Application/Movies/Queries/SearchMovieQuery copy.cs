using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record SearchMoviesHybridQuery(MovieSearchCriteria searchCriteria)
    : IRequest<IReadOnlyList<SearchMovieResultDto>>;

public sealed class SearchMoviesHybridHandler(IMovieTrackRDbContext dbContext, ITmdbClient tmdbClient, IMapper mapper)
    : IRequestHandler<SearchMoviesHybridQuery, IReadOnlyList<SearchMovieResultDto>>
{
    public async Task<IReadOnlyList<SearchMovieResultDto>> Handle(SearchMoviesHybridQuery searchQuery, CancellationToken cancellationToken)
    {
        MovieSearchCriteria searchCriteria = searchQuery.searchCriteria;
        var (Skip, Take) = searchCriteria.Paging();

        IQueryable<Movie> localQuery = dbContext.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchCriteria.Search))
            localQuery = localQuery.Where(m =>
                EF.Functions.ILike(m.Title, $"%{searchCriteria.Search}%") ||
                (m.OriginalTitle != null && EF.Functions.ILike(m.OriginalTitle, $"%{searchCriteria.Search}%")));

        if (searchCriteria.Year is not null) localQuery = localQuery.Where(m => m.Year == searchCriteria.Year);

        if (searchCriteria.GenreId is not null)
            localQuery = localQuery.Where(m => m.MovieGenres.Any(mg => mg.GenreId == searchCriteria.GenreId));

        localQuery = searchCriteria.Sort?.ToLowerInvariant() switch
        {
            "year" => localQuery.OrderByDescending(m => m.Year).ThenBy(m => m.Title),
            "title" => localQuery.OrderBy(m => m.Title),
            _ => localQuery.OrderBy(m => m.Title)
        };

        List<SearchMovieResultDto> locals = await localQuery
            .Skip(Skip).Take(Take)
            .ProjectTo<SearchMovieResultDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var tmdb = await tmdbClient.SearchMoviesAsync(searchCriteria.Search!, searchCriteria.Page, language: null!, region: null, cancellationToken);
        var tmdbDtos = mapper.Map<List<SearchMovieResultDto>>(tmdb.Results);

        return locals;
    }
}