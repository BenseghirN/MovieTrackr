using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Users;

public static class UserProfilesEndpoints
{
    public static IEndpointRouteBuilder MapUserProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/profiles")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("User Profiles")
            .WithOpenApi();

        group.MapGet("/{id:guid}", UserProfilesHandlers.GetById)
            .WithName("UserProfiles_GetById")
            .WithSummary("Récupère le profil public d'un utilisateur.")
            .WithDescription("Retourne uniquement les informations publiques d'un utilisateur.")
            .Produces<PublicUserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/lists", UserProfilesHandlers.GetListsByUser)
            .AllowAnonymous()
            .WithName("Get_Lists_By_UserId")
            .WithSummary("Récupère les listes d'un utilisateur par son identifiant.")
            .WithDescription("Retourne la vue 'résumé' des listes appartenant à un utilisateur.")
            .Produces<IReadOnlyList<UserListDto>>(StatusCodes.Status200OK);
        return app;
    }
}