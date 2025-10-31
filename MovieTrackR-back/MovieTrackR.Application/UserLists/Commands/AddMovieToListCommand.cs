using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.Movies.Commands;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record AddMovieToListCommand(CurrentUserDto currentUser, Guid ListId, Guid? MovieId, int? TmdbId, int? Position) : IRequest;

public sealed class AddMovieToListHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<AddMovieToListCommand>
{
    public async Task Handle(AddMovieToListCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.currentUser), cancellationToken);
        UserList list = await dbContext.UserLists.FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == userId, cancellationToken)
                   ?? throw new ForbiddenException("You do not have permission to modify this list.");

        Guid movieId = await sender.Send(new EnsureLocalMovieCommand(command.MovieId, command.TmdbId), cancellationToken);

        bool exists = await dbContext.UserListMovies.AnyAsync(i => i.UserListId == list.Id && i.MovieId == movieId, cancellationToken);
        if (exists) throw new ConflictException("Movie already exists in the list.");

        Movie movie = await dbContext.Movies.FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken)
            ?? throw new NotFoundException("Movie", movieId);

        int position = command.Position
            ?? ((await dbContext.UserListMovies
                    .Where(i => i.UserListId == list.Id)
                    .Select(i => (int?)i.Position)
                    .MaxAsync(cancellationToken)) ?? 0) + 10;

        list.AddMovie(movie, position);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}