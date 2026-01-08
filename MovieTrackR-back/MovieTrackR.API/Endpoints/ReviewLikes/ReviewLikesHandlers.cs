using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.ReviewLikes.Commands;

namespace MovieTrackR.API.Endpoints.ReviewLikes;

/// <summary>Handlers HTTP pour la feature pour liker une review.</summary>
public static class ReviewLikesHandlers
{
    /// <summary>
    /// Ajoute un like à une review (idempotent).
    /// </summary>
    /// <remarks>Erreurs (middleware) : 401, 404.</remarks>
    public static async Task<NoContent> Like(
        Guid reviewId, ClaimsPrincipal user, IMediator mediator, CancellationToken ct)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await mediator.Send(new LikeReviewCommand(reviewId, current), ct);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Retire le like de l'utilisateur courant (idempotent).
    /// </summary>
    /// <remarks>Erreurs (middleware) : 401.</remarks>
    public static async Task<NoContent> Unlike(
        Guid reviewId, ClaimsPrincipal user, IMediator mediator, CancellationToken ct)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await mediator.Send(new UnlikeReviewCommand(reviewId, current), ct);
        return TypedResults.NoContent();
    }
}