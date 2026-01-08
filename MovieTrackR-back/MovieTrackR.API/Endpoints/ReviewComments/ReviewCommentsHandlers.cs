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
    public static async Task<Ok<IReadOnlyList<CommentDto>>> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        IReadOnlyList<CommentDto> dto = await mediator.Send(new GetAllCommentsQuery(), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<Ok<PagedResult<CommentDto>>> GetComments(
        Guid reviewId, int page, int pageSize, IMediator mediator, CancellationToken cancellationToken)
    {
        PagedResult<CommentDto> result = await mediator.Send(new GetCommentsForReviewQuery(reviewId, page, pageSize), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Created<object>> CreateComment(
        Guid reviewId, ClaimsPrincipal user, CommentCreateDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        Guid commentId = await mediator.Send(new CreateCommentCommand(reviewId, dto, current), cancellationToken);
        return TypedResults.Created<object>($"/api/v1/reviews/{reviewId}/comments/{commentId}", new { id = commentId });
    }

    public static async Task<NoContent> UpdateComment(
        Guid reviewId, Guid commentId, ClaimsPrincipal user, CommentUpdateDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await mediator.Send(new UpdateCommentCommand(reviewId, commentId, dto, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteComment(
        Guid reviewId, Guid commentId, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await mediator.Send(new DeleteCommentCommand(reviewId, commentId, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> UpdateVisibility(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateCommentVisibilityCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}