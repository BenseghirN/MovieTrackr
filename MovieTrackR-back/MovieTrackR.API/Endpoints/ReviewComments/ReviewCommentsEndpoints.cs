using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Configuration;
using MovieTrackR.API.Filters;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.ReviewComments;

public static class ReviewCommentsEndpoints
{
        public static IEndpointRouteBuilder MapReviewCommentsEndpoints(this IEndpointRouteBuilder app)
        {
                ApiVersionSet vset = app.NewApiVersionSet()
                        .HasApiVersion(new ApiVersion(1, 0))
                        .Build();

                RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/reviews/{reviewId:guid}/comments")
                        .WithApiVersionSet(vset)
                        .MapToApiVersion(1, 0)
                        .WithTags("ReviewComments")
                        .WithOpenApi()
                        .RequireAuthorization();

                group.MapGet("", ReviewCommentsHandlers.GetComments)
                        .AllowAnonymous()
                        .WithSummary("List comments for a review")
                        .WithDescription("Returns a paginated list of comments for the specified review.")
                        .Produces<PagedResult<CommentDto>>(StatusCodes.Status200OK);

                group.MapPost("", ReviewCommentsHandlers.CreateComment)
                        .WithSummary("Create a comment")
                        .WithDescription("Creates a new comment on the specified review.")
                        .Accepts<CommentCreateDto>("application/json")
                        .AddEndpointFilter<ValidationFilter<CommentCreateDto>>()
                        .Produces(StatusCodes.Status201Created)
                        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .ProducesProblem(StatusCodes.Status404NotFound);

                group.MapPut("{commentId:guid}", ReviewCommentsHandlers.UpdateComment)
                        .WithSummary("Update a comment")
                        .WithDescription("Updates an existing comment. Only the author or an admin can edit it.")
                        .Accepts<CommentUpdateDto>("application/json")
                        .AddEndpointFilter<ValidationFilter<CommentUpdateDto>>()
                        .Produces(StatusCodes.Status204NoContent)
                        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .ProducesProblem(StatusCodes.Status403Forbidden)
                        .ProducesProblem(StatusCodes.Status404NotFound);

                group.MapDelete("{commentId:guid}", ReviewCommentsHandlers.DeleteComment)
                        .WithSummary("Delete a comment")
                        .WithDescription("Deletes an existing comment. Only the author or an admin can delete it.")
                        .Produces(StatusCodes.Status204NoContent)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .ProducesProblem(StatusCodes.Status403Forbidden)
                        .ProducesProblem(StatusCodes.Status404NotFound);

                RouteGroupBuilder adminGroup = app.MapGroup("/api/v{version:apiVersion}/comments/")
                        .WithApiVersionSet(vset)
                        .MapToApiVersion(1, 0)
                        .WithTags("ReviewComments")
                        .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
                        .WithOpenApi();

                adminGroup.MapGet("/", ReviewCommentsHandlers.GetAll)
                        .WithSummary("List of all comments")
                        .WithDescription("Returns a list of all comments for moderation")
                        .Produces<IReadOnlyList<CommentDto>>(StatusCodes.Status200OK);

                adminGroup.MapPut("/{id:Guid}", ReviewCommentsHandlers.UpdateVisibility)
                        .WithSummary("Change public visibility of the comment (for moderation)")
                        .WithDescription("Change public visibility of the comment (for moderation)")
                        .Produces(StatusCodes.Status204NoContent)
                        .ProducesProblem(StatusCodes.Status404NotFound);

                return app;
        }
}