using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.Auth.Queries;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Auth;

/// <summary>Gère l'authentification de l'utilisateur via Entra External ID (ex-Azure AD B2C).</summary>
public static class AuthHandlers
{
    /// <summary>
    /// Redirige l’utilisateur vers le fournisseur OIDC pour l’authentification.
    /// </summary>
    /// <param name="context">Contexte HTTP.</param>
    /// <param name="config">Configuration applicative.</param>
    /// <param name="returnUrl">URL de redirection après connexion.</param>
    /// <returns>Redirection (302) vers le fournisseur d'identité.</returns>
    public static IResult Connect(HttpContext context, IConfiguration config, string? returnUrl)
    {
        string scheme =
            config["EntraExternalId:openIdScheme"] ??
            config["EntraExternalId:OpenIdScheme"] ??
            throw new InvalidOperationException("OpenId scheme manquant.");

        string? frontOrigin = config["FrontEnd:Origin"];

        string resolvedReturnUrl =
            string.IsNullOrWhiteSpace(returnUrl)
                ? (frontOrigin ?? "/")
                : returnUrl;

        AuthenticationProperties props = new AuthenticationProperties
        {
            RedirectUri = resolvedReturnUrl
        };

        return TypedResults.Challenge(props, [scheme]);
    }

    /// <summary>
    /// Retourne les informations de l’utilisateur actuellement authentifié.
    /// </summary>
    /// <param name="user">Principal de l’utilisateur.</param>
    /// <returns>Liste des claims de l’utilisateur.</returns>
    public static IResult Me(ClaimsPrincipal user)
    {
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToArray();
        return TypedResults.Ok(new { claims });
    }

    /// <summary>
    /// Retourne les informations détaillées de l'utilisateur connecté.
    /// </summary>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="user">Principal de l’utilisateur.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns><see cref="UserDto"/> si trouvé, sinon 404.</returns>
    public static async Task<IResult> GetUserInfo(IMediator mediator, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        CurrentUserDto currentUser = user.ToCurrentUserDto();

        UserDto? response = await mediator.Send(new GetCurrentUserInfoQuery(currentUser), cancellationToken);
        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    /// <summary>
    /// Déconnecte l’utilisateur en supprimant le cookie d’authentification.
    /// </summary>
    /// <param name="context">Contexte HTTP.</param>
    /// <param name="returnUrl">URL de redirection après déconnexion.</param>
    /// <returns>Redirection (302) vers l’URL indiquée.</returns>
    public static async Task<IResult> Logout(HttpContext context, string? returnUrl)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return TypedResults.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}