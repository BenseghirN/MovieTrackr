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
            .WithOpenApi();

        group.MapGet("/", UsersHandlers.GetAll)
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .WithName("Users_GetAll")
            .WithSummary("Récupère tous les utilisateurs.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<List<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", UsersHandlers.GetById)
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .WithName("Users_GetById")
            .WithSummary("Récupère un utilisateur par son identifiant.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UsersHandlers.UpdateUser)
            .RequireAuthorization(AuthorizationConfiguration.UserOwnerPolicy)
            .WithName("Users_Update")
            .WithSummary("Met à jour les informations d'un utilisateur")
            .WithDescription("Réservé aux administrateurs ou à l'utilisateur lui-même")
            .Produces<PublicUserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/avatar", UsersHandlers.UpdateUserAvatar)
            .RequireAuthorization(AuthorizationConfiguration.UserOwnerPolicy)
            .WithName("Users_Update_Avatar")
            .WithSummary("Met à jour l'avatar d'un utilisateur")
            .WithDescription("Réservé aux administrateurs ou à l'utilisateur lui-même")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<PublicUserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/promote", UsersHandlers.PromoteToAdmin)
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .WithName("Users_PromoteToAdmin")
            .WithSummary("Promeut un utilisateur au rôle administrateur.")
            .WithDescription("Réservé aux administrateurs.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/demote", UsersHandlers.DemoteToUser)
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
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