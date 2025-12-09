using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Reviews.Commands;
using MovieTrackR.Application.Reviews.Queries;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.API.Endpoints.Reviews;

/// <summary>Handlers HTTP pour la feature Reviews (Lecture, Écriture, Likes, Commentaires).</summary>
public static class ReviewsHandlers
{
    /// <summary>
    /// Récupère les détails d'une review.
    /// </summary>
    /// <param name="id">Identifiant de la review.</param>
    /// <param name="sender">MediatR sender.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>200 avec <see cref="ReviewDetailsDto"/>.</returns>
    /// <remarks>
    /// <para>Erreurs possibles (laissées au middleware) :</para>
    /// <list type="bullet">
    /// <item><description>404 si la review n'existe pas.</description></item>
    /// </list>
    /// </remarks>
    public static async Task<Ok<ReviewDetailsDto>> GetById(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        ReviewDetailsDto dto = await sender.Send(new GetReviewDetailsQuery(id), cancellationToken);
        return TypedResults.Ok(dto);
    }

    /// <summary>
    /// Liste paginée des reviews pour un film.
    /// </summary>
    public static async Task<Ok<PagedResult<ReviewListItemDto>>> GetByMovie(
            Guid movieId,
            ClaimsPrincipal user,
            int page,
            int pageSize,
            MovieReviewSortOption sort,
            int? ratinFilter,
            ISender sender,
            CancellationToken cancellationToken)
    {
        CurrentUserDto? currentUser = user.Identity?.IsAuthenticated == true
            ? user.ToCurrentUserDto()
            : null;
        PagedResult<ReviewListItemDto> result = await sender.Send(new GetReviewsByMovieQuery(movieId, currentUser, page, pageSize, sort, ratinFilter), cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Liste paginée des reviews d'un utilisateur.
    /// </summary>
    public static async Task<Ok<PagedResult<ReviewListItemDto>>> GetByUser(
            Guid userId,
            ClaimsPrincipal user,
            int page,
            int pageSize,
            UserReviewSortOption sort,
            int? ratingFilter,
            ISender sender,
            CancellationToken cancellationToken)
    {
        CurrentUserDto? currentUser = user.Identity?.IsAuthenticated == true
            ? user.ToCurrentUserDto()
            : null;
        PagedResult<ReviewListItemDto> result = await sender.Send(new GetReviewsByUserQuery(userId, currentUser, page, pageSize, sort, ratingFilter), cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Crée une review pour un film.
    /// </summary>
    /// <param name="user">Utilisateur authentifié (claims Entra ID).</param>
    /// <param name="dto">Payload de création.</param>
    /// <param name="sender">MediatR sender.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>201 Created avec l'ID de la review.</returns>
    /// <remarks>
    /// <para>Erreurs (middleware) : 400 validation, 401 non authentifié, 409 déjà présent.</para>
    /// </remarks>
    public static async Task<Created<object>> Create(ClaimsPrincipal user, CreateReviewDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        Guid id = await sender.Send(new CreateReviewCommand(dto, current), cancellationToken);
        return TypedResults.Created<object>($"/api/v1/reviews/{id}", new { id });
    }

    /// <summary>
    /// Met à jour une review (note/texte).
    /// </summary>
    /// <remarks>Erreurs (middleware) : 400, 401, 403, 404.</remarks>
    public static async Task<NoContent> Update(Guid id, ClaimsPrincipal user, UpdateReviewDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new UpdateReviewCommand(id, dto, current), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Supprime une review.
    /// </summary>
    /// <remarks>Erreurs (middleware) : 401, 403, 404.</remarks>
    public static async Task<NoContent> Delete(Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new DeleteReviewCommand(id, current), cancellationToken);
        return TypedResults.NoContent();
    }
}