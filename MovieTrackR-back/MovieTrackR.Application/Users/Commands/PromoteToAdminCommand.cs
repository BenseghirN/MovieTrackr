using AutoMapper;
using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.Users.Commands;

public sealed record PromoteToAdminCommand(Guid Id) : IRequest<UserDto>;

internal sealed class PromoteToAdminHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<PromoteToAdminCommand, UserDto>
{
    public async Task<UserDto> Handle(PromoteToAdminCommand command, CancellationToken cancellationToken)
    {
        User user = await dbContext.Users.FindAsync([command.Id], cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        if (user.Role != UserRole.Admin)
        {
            user.PromoteToAdmin();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return mapper.Map<UserDto>(user);
    }
}