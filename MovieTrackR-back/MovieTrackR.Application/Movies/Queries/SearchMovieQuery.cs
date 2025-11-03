using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record SearchMoviesQuery(MovieSearchCriteria searchCriteria)
    : IRequest<(IReadOnlyList<MovieDto> Items, int Total)>;

public sealed class SearchMoviesHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<SearchMoviesQuery, (IReadOnlyList<MovieDto>, int)>
{
    public async Task<(IReadOnlyList<MovieDto>, int)> Handle(SearchMoviesQuery searchQuery, CancellationToken cancellationToken)
    {
        MovieSearchCriteria searchCriteria = searchQuery.searchCriteria;
        IQueryable<Movie> query = dbContext.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchCriteria.Query))
            query = query.Where(m =>
                EF.Functions.ILike(m.Title, $"%{searchCriteria.Query}%") ||
                (m.OriginalTitle != null && EF.Functions.ILike(m.OriginalTitle, $"%{searchCriteria.Query}%")));

        if (searchCriteria.Year is not null) query = query.Where(m => m.Year == searchCriteria.Year);

        if (searchCriteria.GenreId is not null)
            query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == searchCriteria.GenreId));

        query = searchCriteria.Sort?.ToLowerInvariant() switch
        {
            "year" => query.OrderByDescending(m => m.Year).ThenBy(m => m.Title),
            "title" => query.OrderBy(m => m.Title),
            _ => query.OrderBy(m => m.Title)
        };

        int total = await query.CountAsync(cancellationToken);
        List<Movie> movieResult = await query.Skip((searchCriteria.Page - 1) * searchCriteria.PageSize).Take(searchCriteria.PageSize).ToListAsync(cancellationToken);
        return (mapper.Map<IReadOnlyList<MovieDto>>(movieResult), total);
    }
}