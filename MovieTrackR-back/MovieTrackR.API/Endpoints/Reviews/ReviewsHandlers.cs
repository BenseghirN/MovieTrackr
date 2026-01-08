using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Reviews.Commands;
using MovieTrackR.Application.Reviews.Queries;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.API.Endpoints.Reviews;

public static class ReviewsHandlers
{
    public static async Task<Ok<IReadOnlyList<ReviewListItemDto>>> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        IReadOnlyList<ReviewListItemDto> dto = await mediator.Send(new GetAllReviewsQuery(), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<Ok<ReviewDetailsDto>> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        ReviewDetailsDto dto = await mediator.Send(new GetReviewDetailsQuery(id), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<Ok<PagedResult<ReviewListItemDto>>> GetByMovie(
            Guid movieId,
            ClaimsPrincipal user,
            int page,
            int pageSize,
            MovieReviewSortOption sort,
            int? ratinFilter,
            IMediator mediator,
            CancellationToken cancellationToken)
    {
        CurrentUserDto? currentUser = user.Identity?.IsAuthenticated == true
            ? user.ToCurrentUserDto()
            : null;
        PagedResult<ReviewListItemDto> result = await mediator.Send(new GetReviewsByMovieQuery(movieId, currentUser, page, pageSize, sort, ratinFilter), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<PagedResult<ReviewListItemDto>>> GetByUser(
            Guid userId,
            ClaimsPrincipal user,
            int page,
            int pageSize,
            UserReviewSortOption sort,
            int? ratingFilter,
            IMediator mediator,
            CancellationToken cancellationToken)
    {
        CurrentUserDto? currentUser = user.Identity?.IsAuthenticated == true
            ? user.ToCurrentUserDto()
            : null;
        PagedResult<ReviewListItemDto> result = await mediator.Send(new GetReviewsByUserQuery(userId, currentUser, page, pageSize, sort, ratingFilter), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Created<object>> Create(ClaimsPrincipal user, CreateReviewDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        Guid id = await mediator.Send(new CreateReviewCommand(dto, current), cancellationToken);
        return TypedResults.Created<object>($"/api/v1/reviews/{id}", new { id });
    }

    public static async Task<NoContent> Update(Guid id, ClaimsPrincipal user, UpdateReviewDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await mediator.Send(new UpdateReviewCommand(id, dto, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> Delete(Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await mediator.Send(new DeleteReviewCommand(id, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> UpdateVisibility(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        ReviewListItemDto? dto = await mediator.Send(new UpdateReviewVisibilityCommand(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }
}