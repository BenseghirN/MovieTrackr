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
    public static async Task<Ok<IReadOnlyList<ReviewListItemDto>>> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        IReadOnlyList<ReviewListItemDto> dto = await sender.Send(new GetAllReviewsQuery(), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<Ok<ReviewDetailsDto>> GetById(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        ReviewDetailsDto dto = await sender.Send(new GetReviewDetailsQuery(id), cancellationToken);
        return TypedResults.Ok(dto);
    }

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

    public static async Task<Created<object>> Create(ClaimsPrincipal user, CreateReviewDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        Guid id = await sender.Send(new CreateReviewCommand(dto, current), cancellationToken);
        return TypedResults.Created<object>($"/api/v1/reviews/{id}", new { id });
    }

    public static async Task<NoContent> Update(Guid id, ClaimsPrincipal user, UpdateReviewDto dto, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new UpdateReviewCommand(id, dto, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> Delete(Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        await sender.Send(new DeleteReviewCommand(id, current), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> UpdateVisibility(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        ReviewListItemDto? dto = await sender.Send(new UpdateReviewVisibilityCommand(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }
}