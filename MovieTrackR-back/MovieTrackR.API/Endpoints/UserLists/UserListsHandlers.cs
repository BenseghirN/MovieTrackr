using System.Security.Claims;
using MediatR;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.UserLists.Commands;
using MovieTrackR.Application.UserLists.Queries;

namespace MovieTrackR.API.Endpoints.UserLists;

/// <summary>Handlers HTTP pour la gestion des listes utilisateur (création, lecture, MAJ, suppression et gestion des éléments).</summary>
public static class UserListsHandlers
{
    /// <summary>Récupère les listes (vue "résumé") de l'utilisateur courant.</summary>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 200 avec la collection de <see cref="UserListDto"/>.
    /// </returns>
    public static async Task<IResult> GetMine(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        IReadOnlyList<UserListDto> result = await sender.Send(new GetMyListsQuery(currentUser), cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>Récupère le détail d'une liste appartenant à l'utilisateur courant.</summary>
    /// <param name="listId">Identifiant de la liste.</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 200 avec <see cref="UserListDetailsDto"/> si trouvé; 404 sinon.
    /// </returns>
    public static async Task<IResult> GetById(Guid listId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        UserListDetailsDto? userList = await sender.Send(new GetListByIdQuery(currentUser, listId), cancellationToken);
        return userList is null ? TypedResults.NotFound() : TypedResults.Ok(userList);
    }

    /// <summary>Crée une nouvelle liste pour l'utilisateur courant.</summary>
    /// <param name="newList">Données d'entrée (titre/description).</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 201 Created avec l'URI de la ressource et l'identifiant créé.
    /// </returns>
    public static async Task<IResult> Create(CreateListDto newList, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        Guid id = await sender.Send(new CreateListCommand(currentUser, newList.Title, newList.Description ?? string.Empty), cancellationToken);
        return TypedResults.Created($"/lists/{id}", new { id });
    }

    /// <summary>Met à jour le titre/description d’une liste de l'utilisateur courant.</summary>
    /// <param name="listId">Identifiant de la liste.</param>
    /// <param name="updatedList">Nouvelles valeurs (titre/description).</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 204 No Content si OK; 404 si la liste est introuvable.
    /// </returns>
    public static async Task<IResult> Update(Guid listId, UpdateListDto updatedList, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new UpdateListCommand(currentUser, listId, updatedList.Title, updatedList.Description ?? string.Empty), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Supprime une liste de l'utilisateur courant.</summary>
    /// <param name="listId">Identifiant de la liste.</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 204 No Content si supprimée; 404 si introuvable.
    /// </returns>
    public static async Task<IResult> Delete(Guid listId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new DeleteListCommand(currentUser, listId), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Retire un film d’une liste de l'utilisateur courant.</summary>
    /// <param name="listId">Identifiant de la liste.</param>
    /// <param name="movieId">Identifiant du film (local GUID) à retirer.</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 204 No Content si retiré; 404 si l’élément n’existe pas.
    /// </returns>
    public static async Task<IResult> RemoveItem(Guid listId, Guid movieId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new RemoveMovieFromListCommand(currentUser, listId, movieId), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Ajoute un film à une liste (import TMDb au besoin).</summary>
    /// <param name="listId">Identifiant de la liste.</param>
    /// <param name="movieToAdd">Film à ajouter (MovieId local ou TmdbId, position optionnelle).</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 204 No Content (opération idempotente).
    /// </returns>
    public static async Task<IResult> AddItem(Guid listId, AddMovieToListDto movieToAdd, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new AddMovieToListCommand(currentUser, listId, movieToAdd.MovieId, movieToAdd.TmdbId, movieToAdd.Position), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Modifie la position d’un film dans une liste.</summary>
    /// <param name="listId">Identifiant de la liste.</param>
    /// <param name="movie">Identifiant du film et nouvelle position.</param>
    /// <param name="user">Principal (claims) de la requête.</param>
    /// <param name="sender">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>
    /// 204 No Content si OK; 404 si l’élément est introuvable.
    /// </returns>
    public static async Task<IResult> ReorderItem(Guid listId, ReorderListItemDto movie, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();
        await sender.Send(new ReorderListItemCommand(currentUser, listId, movie.MovieId, movie.NewPosition), cancellationToken);
        return TypedResults.NoContent();
    }
}