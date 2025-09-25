using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record RemoveMovieFromListCommand(Guid UserId, Guid ListId, Guid MovieId) : IRequest;

public sealed class RemoveMovieFromListHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<RemoveMovieFromListCommand>
{
    public async Task Handle(RemoveMovieFromListCommand command, CancellationToken cancellationToken)
    {
        UserList list = await dbContext.UserLists
            .Include(l => l.Movies)
            .FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("UserList", command.ListId);

        if (!list.Movies.Any(m => m.MovieId == command.MovieId))
            throw new NotFoundException("UserListMovie", command.MovieId);

        list.RemoveMovie(command.MovieId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}