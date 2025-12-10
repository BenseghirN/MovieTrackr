using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetAllMoviesQuery()
    : IRequest<IReadOnlyList<MovieDetailsDto>>;

public sealed class GetAllMoviesHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllMoviesQuery, IReadOnlyList<MovieDetailsDto>>
{
    public async Task<IReadOnlyList<MovieDetailsDto>> Handle(GetAllMoviesQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Movies
            .AsNoTracking()
            .AsSplitQuery()
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .OrderBy(m => m.Title).ThenBy(m => m.CreatedAt)
            .ProjectTo<MovieDetailsDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}