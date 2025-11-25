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

        int total = await query.CountAsync(cancellationToken);
        List<Movie> movieResult = await query.Skip((searchCriteria.Page - 1) * searchCriteria.PageSize).Take(searchCriteria.PageSize).ToListAsync(cancellationToken);
        return (mapper.Map<IReadOnlyList<MovieDto>>(movieResult), total);
    }
}