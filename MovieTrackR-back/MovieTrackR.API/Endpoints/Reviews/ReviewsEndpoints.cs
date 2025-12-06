using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Filters;
using MovieTrackR.API.Validators.Reviews;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Reviews;

public static class ReviewsEndpoints
{
  public static IEndpointRouteBuilder MapReviewsEndpoints(this IEndpointRouteBuilder app)
  {
    ApiVersionSet vset = app.NewApiVersionSet()
      .HasApiVersion(new ApiVersion(1, 0))
      .Build();

    RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/reviews")
      .WithApiVersionSet(vset)
      .MapToApiVersion(1, 0)
      .WithTags("Reviews")
      .WithOpenApi();

    group.MapGet("{id:guid}", ReviewsHandlers.GetById)
      .AllowAnonymous()
      .WithSummary("Get review details")
      .WithDescription("Returns the full details of a review by its identifier.")
      .Produces<ReviewDetailsDto>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status404NotFound);

    group.MapGet("by-movie/{movieId:guid}", ReviewsHandlers.GetByMovie)
      .AllowAnonymous()
      .WithSummary("List reviews for a movie")
      .WithDescription("Returns a paginated, sortable and filterable list of reviews for the given movie.")
      .Produces<PagedResult<ReviewListItemDto>>(StatusCodes.Status200OK);

    group.MapGet("by-user/{userId:guid}", ReviewsHandlers.GetByUser)
      .AllowAnonymous()
      .WithSummary("List reviews by user")
      .WithDescription("Returns a paginated, sortable and filterable list of reviews for the given user.")
      .Produces<PagedResult<ReviewListItemDto>>(StatusCodes.Status200OK);

    group.MapPost("", ReviewsHandlers.Create)
      .WithSummary("Create a review")
      .WithDescription("Creates a new review. A user can create only one review per movie.")
      .Accepts<CreateReviewDto>("application/json")
      .AddEndpointFilter<ValidationFilter<CreateReviewDto>>()
      .Produces(StatusCodes.Status201Created)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapPut("{id:guid}", ReviewsHandlers.Update)
      .WithSummary("Update a review")
      .WithDescription("Updates an existing review. Only the author or an admin can update it.")
      .Accepts<UpdateReviewDto>("application/json")
      .AddEndpointFilter<ValidationFilter<UpdateReviewDto>>()
      .Produces(StatusCodes.Status204NoContent)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status404NotFound);

    group.MapDelete("{id:guid}", ReviewsHandlers.Delete)
      .WithSummary("Delete a review")
      .WithDescription("Deletes a review. Only the author or an admin can delete it.")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status404NotFound);

    return app;
  }
}