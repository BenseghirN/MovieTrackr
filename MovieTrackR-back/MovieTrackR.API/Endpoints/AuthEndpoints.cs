using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MovieTrackR.API.Endpoints;

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
            .WithTags("Auth");

        // GET /connect -> challenge OIDC
        group.MapGet("/connect", (HttpContext ctx, IConfiguration config, string? returnUrl) =>
        {
            string scheme =
                config["EntraExternalId:openIdScheme"] ??
                config["EntraExternalId:OpenIdScheme"] ??
                throw new InvalidOperationException("OpenId scheme manquant (EntraExternalId:openIdScheme).");

            AuthenticationProperties props = new AuthenticationProperties { RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl };
            return Results.Challenge(props, [scheme]);
        })
        .AllowAnonymous()
        .Produces(StatusCodes.Status302Found);

        // GET /me -> claims
        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToArray();
            return Results.Ok(new { claims });
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // GET /user-info -> UserDto via service
        // group.MapGet("/user-info", async (IUserSynchronizationService sync, CancellationToken ct) =>
        // {
        //     var user = await sync.GetCurrentUserInfosAsync(ct);
        //     return user is null ? Results.NotFound() : Results.Ok(user);
        // })
        // .RequireAuthorization()
        // .Produces<UserDto>(StatusCodes.Status200OK)
        // .Produces(StatusCodes.Status401Unauthorized)
        // .Produces(StatusCodes.Status404NotFound);

        // GET /logout -> supprime cookie & redirige
        group.MapGet("/logout", async (HttpContext ctx, string? returnUrl) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        })
        .AllowAnonymous()
        .Produces(StatusCodes.Status302Found);

        return app;
    }
}