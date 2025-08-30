using System.Security.Claims;
using AutoMapper;
using MediatR;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Auth.Queries;

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