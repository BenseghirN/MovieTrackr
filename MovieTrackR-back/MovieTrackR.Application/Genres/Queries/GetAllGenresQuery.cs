
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Genres.Queries;

public sealed record GetAllGenresQuery() : IRequest<IReadOnlyList<GenreDto>>;

public sealed class GetAllGenresHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllGenresQuery, IReadOnlyList<GenreDto>>
{
    public async Task<IReadOnlyList<GenreDto>> Handle(GetAllGenresQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Genres
            .OrderBy(g => g.Name)
            .ProjectTo<GenreDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}