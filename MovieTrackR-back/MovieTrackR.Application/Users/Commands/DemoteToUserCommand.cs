using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.Users.Commands;

public sealed record DemoteToUserCommand(Guid Id) : IRequest<UserDto>;

internal sealed class DemoteToUserHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<DemoteToUserCommand, UserDto>
{
    public async Task<UserDto> Handle(DemoteToUserCommand command, CancellationToken cancellationToken)
    {
        User user = await dbContext.Users.FindAsync(new object[] { command.Id }, cancellationToken)
                        ?? throw new KeyNotFoundException("User not found");

        if (user.Role != UserRole.Admin)
        {
            user.DemoteToUser();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return mapper.Map<UserDto>(user);
    }
}