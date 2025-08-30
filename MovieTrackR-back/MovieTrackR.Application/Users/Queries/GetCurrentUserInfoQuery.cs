using System.Security.Claims;
using AutoMapper;
using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Users.Commands;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Users.Queries;

public sealed record GetCurrentUserInfoQuery(ClaimsPrincipal User) : IRequest<UserDto?>;

public sealed class GetCurrentUserInfoHandler(IMediator mediator, IMapper mapper)
    : IRequestHandler<GetCurrentUserInfoQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
    {
        User user = await mediator.Send(new EnsureUserExistsCommand(request.User), cancellationToken);
        return mapper.Map<UserDto>(user);
    }
}