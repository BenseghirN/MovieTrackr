using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.OpenApi.Models;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("Auth")
            .WithOpenApi();

        group.MapGet("/connect", AuthHandlers.Connect)
        .AllowAnonymous()
        .WithName("Auth_Connect")
        .WithSummary("Redirige l’utilisateur vers Entra (OIDC) pour l’authentification.")
        .WithDescription("Retourne une redirection 302 vers le fournisseur d'identité. " +
                        "Utilise 'returnUrl' pour la page finale après le callback '/signin-oidc'.")
        .Produces(StatusCodes.Status302Found)
        .WithOpenApi(op =>
        {
            OpenApiParameter? p = op.Parameters.FirstOrDefault(x => x.Name == "returnUrl");
            if (p != null) p.Description = "URL de redirection après connexion.";
            return op;
        });

        group.MapGet("/me", AuthHandlers.Me)
        .RequireAuthorization()
        .WithName("Auth_Me")
        .WithSummary("Retourne les claims de l’utilisateur authentifié.")
        .WithDescription("Nécessite une session valide. Retourne 200 avec la liste des claims ; 401 si non authentifié.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/user-info", AuthHandlers.GetUserInfo)
        .RequireAuthorization()
        .WithName("Auth_UserInfo")
        .WithSummary("Retourne les informations détaillées de l’utilisateur connecté.")
        .WithDescription("Renvoie un objet UserDto. 404 si l’utilisateur n’existe pas côté application.")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/logout", AuthHandlers.Logout)
        .AllowAnonymous()
        .WithName("Auth_Logout")
        .WithSummary("Déconnecte l’utilisateur (suppression du cookie) et redirige.")
        .WithDescription("Retourne une redirection 302 vers 'returnUrl' (ou '/').")
        .Produces(StatusCodes.Status302Found)
        .WithOpenApi(op =>
        {
            OpenApiParameter? p = op.Parameters.FirstOrDefault(x => x.Name == "returnUrl");
            if (p != null) p.Description = "URL de redirection après déconnexion.";
            return op;
        });

        return app;
    }
}