using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record CreateListCommand(CurrentUserDto currentUser, string Title, string Description) : IRequest<Guid>;

public sealed class CreateListHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<CreateListCommand, Guid>
{
    public async Task<Guid> Handle(CreateListCommand request, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(request.currentUser), cancellationToken);

        bool exists = await dbContext.UserLists.AnyAsync(l => l.UserId == userId && l.Title == request.Title, cancellationToken);
        if (exists) throw new ConflictException("List title already exists.");

        UserList list = UserList.Create(userId, request.Title, request.Description);
        dbContext.UserLists.Add(list);
        await dbContext.SaveChangesAsync(cancellationToken);

        return list.Id;
    }
}