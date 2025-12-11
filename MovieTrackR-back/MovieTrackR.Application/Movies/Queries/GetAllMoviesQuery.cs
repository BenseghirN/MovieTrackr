using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetAllMoviesQuery()
    : IRequest<IReadOnlyList<MovieAdminDto>>;

public sealed class GetAllMoviesHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllMoviesQuery, IReadOnlyList<MovieAdminDto>>
{
    public async Task<IReadOnlyList<MovieAdminDto>> Handle(GetAllMoviesQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Movies
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(m => m.Title).ThenBy(m => m.CreatedAt)
            .ProjectTo<MovieAdminDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}