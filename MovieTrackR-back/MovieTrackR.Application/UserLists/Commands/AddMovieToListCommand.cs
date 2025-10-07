using MediatR;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record AddMovieToListCommand(CurrentUserDto currentUser, Guid ListId, Guid? MovieId, int? TmdbId, int? Position) : IRequest;

public sealed class AddMovieToListHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<AddMovieToListCommand>
{
    public async Task Handle(AddMovieToListCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.currentUser), cancellationToken);
        // UserList list = await dbContext.UserLists.FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == command.UserId, cancellationToken)
        //            ?? throw new NotFoundException("List", command.ListId);

        // var movieId = await sender.Send(new EnsureLocalMovieCommand(command.MovieId, command.TmdbId), cancellationToken);

        // bool exists = await dbContext.UserListItems.AnyAsync(i => i.ListId == list.Id && i.MovieId == movieId, cancellationToken);
        // if (!exists)
        // {
        //     int pos = (await dbContext.UserListItems.Where(i => i.ListId == list.Id)
        //                      .Select(i => (int?)i.Position).DefaultIfEmpty(0).MaxAsync(cancellationToken) ?? 0) + 10;

        //     dbContext.UserListItems.Add(new UserListItem { Id = Guid.NewGuid(), ListId = list.Id, MovieId = movieId, Position = command.Position ?? pos });
        //     await dbContext.SaveChangesAsync(cancellationToken);
        // }
    }
}