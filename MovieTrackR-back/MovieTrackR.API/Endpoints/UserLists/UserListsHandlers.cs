using System.Security.Claims;
using MediatR;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.UserLists.Commands;
using MovieTrackR.Application.UserLists.Queries;

namespace MovieTrackR.API.Endpoints.UserLists;

public static class UserListsHandlers
{
    public static async Task<IResult> GetMine(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        IReadOnlyList<UserListDto> result = await sender.Send(new GetMyListsQuery(currentUser), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetById(Guid listId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        UserListDetailsDto? userList = await sender.Send(new GetListByIdQuery(currentUser, listId), cancellationToken);
        return userList is null ? TypedResults.NotFound() : TypedResults.Ok(userList);
    }

    public static async Task<IResult> Create(CreateListDto newList, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        Guid id = await sender.Send(new CreateListCommand(currentUser, newList.Title, newList.Description ?? string.Empty), cancellationToken);
        return TypedResults.Created($"/lists/{id}", new { id });
    }

    public static async Task<IResult> Update(Guid listId, UpdateListDto updatedList, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new UpdateListCommand(currentUser, listId, updatedList.Title, updatedList.Description ?? string.Empty), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> Delete(Guid listId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new DeleteListCommand(currentUser, listId), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> RemoveItem(Guid listId, Guid movieId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new RemoveMovieFromListCommand(currentUser, listId, movieId), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> AddItem(Guid listId, AddMovieToListDto movieToAdd, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new AddMovieToListCommand(currentUser, listId, movieToAdd.MovieId, movieToAdd.TmdbId, movieToAdd.Position), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> ReorderItem(Guid listId, ReorderListItemDto movie, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new ReorderListItemCommand(currentUser, listId, movie.MovieId, movie.NewPosition), cancellationToken);
        return TypedResults.NoContent();
    }
}