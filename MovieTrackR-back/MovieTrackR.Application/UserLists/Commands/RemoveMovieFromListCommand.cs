using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record RemoveMovieFromListCommand(CurrentUserDto currentUser, Guid ListId, Guid MovieId) : IRequest;

public sealed class RemoveMovieFromListHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<RemoveMovieFromListCommand>
{
    public async Task Handle(RemoveMovieFromListCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.currentUser), cancellationToken);

        UserList list = await dbContext.UserLists
            .Include(l => l.Movies)
            .FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("UserList", command.ListId);

        if (!list.Movies.Any(m => m.MovieId == command.MovieId))
            throw new NotFoundException("UserListMovie", command.MovieId);

        list.RemoveMovie(command.MovieId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}