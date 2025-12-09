using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.ReviewComments.Commands;
using MovieTrackR.Application.ReviewComments.Queries;

namespace MovieTrackR.API.Endpoints.ReviewComments;

/// <summary>Handlers HTTP pour la feature ReviewsComments.</summary>
public static class ReviewCommentsHandlers
{
    /// <summary>
    /// Liste paginée des commentaires d'une review.
    /// </summary>
    public static async Task<Ok<PagedResult<CommentDto>>> GetComments(
        Guid reviewId, int page, int pageSize, ISender sender, CancellationToken cancellationToken)
    {
        PagedResult<CommentDto> result = await sender.Send(new GetCommentsForReviewQuery(reviewId, page, pageSize), cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Crée un commentaire sur une review.
    /// </summary>
    /// <remarks>Erreurs (middleware) : 400, 401, 404.</remarks>
    public static async Task<Created<object>> CreateComment(
        Guid reviewId, ClaimsPrincipal user, CommentCreateDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        Guid commentId = await sender.Send(new CreateCommentCommand(reviewId, dto, current), cancellationToken);
        return TypedResults.Created<object>($"/api/v1/reviews/{reviewId}/comments/{commentId}", new { id = commentId });
    }

    /// <summary>
    /// Met à jour un commentaire.
    /// </summary>
    /// <remarks>Erreurs (middleware) : 400, 401, 403, 404.</remarks>
    public static async Task<NoContent> UpdateComment(
        Guid reviewId, Guid commentId, ClaimsPrincipal user, CommentUpdateDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new UpdateCommentCommand(reviewId, commentId, dto, current), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Supprime un commentaire.
    /// </summary>
    /// <remarks>Erreurs (middleware) : 401, 403, 404.</remarks>
    public static async Task<NoContent> DeleteComment(
        Guid reviewId, Guid commentId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new DeleteCommentCommand(reviewId, commentId, current), cancellationToken);
        return TypedResults.NoContent();
    }
}