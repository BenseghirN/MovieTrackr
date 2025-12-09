using AutoMapper;
using MediatR;
using MovieTrackR.Application.AzureStorage.Interfaces;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

public sealed record UpdateUserAvatarCommand(Guid Id, CurrentUserDto CurrentUser, byte[] Content, string ContentType) : IRequest<PublicUserProfileDto>;

internal sealed class UpdateUserAvatarHandler(IMovieTrackRDbContext dbContext, IAvatarStorageService storageService, IMapper mapper, IMediator mediator)
    : IRequestHandler<UpdateUserAvatarCommand, PublicUserProfileDto>
{
    public async Task<PublicUserProfileDto> Handle(UpdateUserAvatarCommand command, CancellationToken cancellationToken)
    {
        User user = await dbContext.Users.FindAsync([command.Id], cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        Guid userId = await mediator.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        if (user.Id != userId) throw new UnauthorizedAccessException("You are not allowed to update this user.");

        string newAvatarUrl = await storageService.UploadAvatarAsync(
            userId,
            command.Content,
            command.ContentType,
            cancellationToken);

        if (!string.Equals(user.AvatarUrl, newAvatarUrl, StringComparison.Ordinal))
            user.SetAvatar(newAvatarUrl);

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<PublicUserProfileDto>(user);
    }
}