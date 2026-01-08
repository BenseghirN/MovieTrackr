using AutoMapper;
using MediatR;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

public sealed record UpdateUserInfosCommand(Guid Id, CurrentUserDto CurrentUser, UserUpdateDto UpdatedInfos) : IRequest<PublicUserProfileDto>;

internal sealed class UpdateUserInfosHandler(IMovieTrackRDbContext dbContext, IMapper mapper, IMediator mediator)
    : IRequestHandler<UpdateUserInfosCommand, PublicUserProfileDto>
{
    public async Task<PublicUserProfileDto> Handle(UpdateUserInfosCommand command, CancellationToken cancellationToken)
    {
        User user = await dbContext.Users.FindAsync([command.Id], cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        Guid userId = await mediator.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        if (user.Id != userId) throw new UnauthorizedAccessException("You are not allowed to update this user.");

        UserUpdateDto newData = command.UpdatedInfos;

        user.UpdateProfile(
            newData.Pseudo,
            newData.GivenName,
            newData.Surname);

        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<PublicUserProfileDto>(user);
    }
}