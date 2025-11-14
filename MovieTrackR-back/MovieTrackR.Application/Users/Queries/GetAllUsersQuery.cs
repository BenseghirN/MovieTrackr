using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Users.Queries;

public sealed record GetAllUsersQuery : IRequest<List<UserDto>>;

internal sealed class GetAllUsersHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking()
            .OrderBy(u => u.Surname).ThenBy(u => u.GivenName)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}