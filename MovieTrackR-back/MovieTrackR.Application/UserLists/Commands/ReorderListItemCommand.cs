using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record ReorderListItemCommand(CurrentUserDto currentUser, Guid listId, Guid movieId, int newPosition) : IRequest;

public sealed class ReorderListItemHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<ReorderListItemCommand>
{
    public async Task Handle(ReorderListItemCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.currentUser), cancellationToken);

        UserList list = await dbContext.UserLists
            .Include(l => l.Movies)
            .FirstOrDefaultAsync(l => l.Id == command.listId && l.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("UserList", command.listId);

        if (!list.Movies.Any(m => m.MovieId == command.movieId))
            throw new NotFoundException("UserListMovie", command.movieId);

        list.ReorderMovie(command.movieId, command.newPosition);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}