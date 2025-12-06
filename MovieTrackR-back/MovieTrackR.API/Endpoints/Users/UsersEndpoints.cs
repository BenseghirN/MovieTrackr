using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Configuration;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/users")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("Users")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .WithOpenApi();

        group.MapGet("/", UsersHandlers.GetAll)
            .WithName("Users_GetAll")
            .WithSummary("Récupère tous les utilisateurs.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<List<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", UsersHandlers.GetById)
            .WithName("Users_GetById")
            .WithSummary("Récupère un utilisateur par son identifiant.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/promote", UsersHandlers.PromoteToAdmin)
            .WithName("Users_PromoteToAdmin")
            .WithSummary("Promeut un utilisateur au rôle administrateur.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/demote", UsersHandlers.DemoteToUser)
            .WithName("Users_DemoteToUser")
            .WithSummary("Rétrograde un administrateur vers le rôle utilisateur.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}