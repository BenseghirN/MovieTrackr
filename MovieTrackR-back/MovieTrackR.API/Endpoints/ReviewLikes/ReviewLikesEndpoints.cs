using Asp.Versioning;
using Asp.Versioning.Builder;

namespace MovieTrackR.API.Endpoints.ReviewLikes;

public static class ReviewLikesEndpoints
{
  public static IEndpointRouteBuilder MapReviewLikesEndpoints(this IEndpointRouteBuilder app)
  {
    ApiVersionSet vset = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1, 0))
        .Build();

    RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/reviews/{reviewId:guid}/likes")
        .WithApiVersionSet(vset)
        .MapToApiVersion(1, 0)
        .WithTags("ReviewLikes")
        .WithOpenApi()
        .RequireAuthorization();

    group.MapPost("", ReviewLikesHandlers.Like)
     .WithSummary("Like a review")
     .WithDescription("Adds a like to the specified review. Operation is idempotent.")
     .Produces(StatusCodes.Status204NoContent)
     .ProducesProblem(StatusCodes.Status401Unauthorized)
     .ProducesProblem(StatusCodes.Status404NotFound);

    group.MapDelete("", ReviewLikesHandlers.Unlike)
         .WithSummary("Remove like from a review")
         .WithDescription("Removes the current user's like. Operation is idempotent.")
         .Produces(StatusCodes.Status204NoContent)
         .ProducesProblem(StatusCodes.Status401Unauthorized);

    return app;
  }
}