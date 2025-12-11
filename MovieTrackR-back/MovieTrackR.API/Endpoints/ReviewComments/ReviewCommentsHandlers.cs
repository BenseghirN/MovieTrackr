using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.ReviewComments.Commands;
using MovieTrackR.Application.ReviewComments.Queries;

namespace MovieTrackR.API.Endpoints.ReviewComments;

public static class ReviewCommentsHandlers
{
    public static async Task<Ok<IReadOnlyList<CommentDto>>> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        IReadOnlyList<CommentDto> dto = await sender.Send(new GetAllCommentsQuery(), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<Ok<PagedResult<CommentDto>>> GetComments(
        Guid reviewId, int page, int pageSize, ISender sender, CancellationToken cancellationToken)
    {
        PagedResult<CommentDto> result = await sender.Send(new GetCommentsForReviewQuery(reviewId, page, pageSize), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Created<object>> CreateComment(
        Guid reviewId, ClaimsPrincipal user, CommentCreateDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        Guid commentId = await sender.Send(new CreateCommentCommand(reviewId, dto, current), cancellationToken);
        return TypedResults.Created<object>($"/api/v1/reviews/{reviewId}/comments/{commentId}", new { id = commentId });
    }

    public static async Task<NoContent> UpdateComment(
        Guid reviewId, Guid commentId, ClaimsPrincipal user, CommentUpdateDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new UpdateCommentCommand(reviewId, commentId, dto, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteComment(
        Guid reviewId, Guid commentId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new DeleteCommentCommand(reviewId, commentId, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> UpdateVisibility(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateCommentVisibilityCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}