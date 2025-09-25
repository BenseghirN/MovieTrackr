using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Auth.Queries;

public sealed record GetCurrentUserInfoQuery(string externalId, string email, string display, string given, string surname, string pseudo) : IRequest<UserDto?>;

public sealed class GetCurrentUserInfoHandler(IMovieTrackRDbContext dbContext, IMediator mediator, IMapper mapper)
    : IRequestHandler<GetCurrentUserInfoQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
    {
        Guid userId = await mediator.Send(new EnsureUserExistsCommand(request.externalId, request.email, request.display, request.given, request.surname, request.pseudo), cancellationToken);

        User user = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        return mapper.Map<UserDto>(user);
    }
}