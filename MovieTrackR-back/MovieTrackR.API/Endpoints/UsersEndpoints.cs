using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using MovieTrackR.Application.Users.Queries;

namespace MovieTrackR.API.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/users")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("Users")
            .RequireAuthorization();

        // GET /api/v1/users/me
        group.MapGet("/me", async (IMediator mediator, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var dto = await mediator.Send(new GetCurrentUserInfoQuery(user), ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        })
        .Produces<MovieTrackR.Application.DTOs.UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}