using System.Security.Claims;

namespace MovieTrackR.Application.Common.Security;

public static class ClaimsExtensions
{
    private static string? GetClaim(this ClaimsPrincipal user, params string[] claimTypes) =>
        claimTypes
            .Select(t => user.FindFirst(t)?.Value)
            .FirstOrDefault(v => !string.IsNullOrEmpty(v));

    public static string? GetExternalId(this ClaimsPrincipal user) =>
        user.GetClaim(
            "oid",
            "http://schemas.microsoft.com/identity/claims/objectidentifier",
            ClaimTypes.NameIdentifier
        );

    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.GetClaim(
            "email",
            "preferred_username",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            ClaimTypes.Email
        );

    public static string? GetDisplayName(this ClaimsPrincipal user) =>
        user.GetClaim("name", ClaimTypes.Name);

    public static string? GetGivenName(this ClaimsPrincipal user) =>
        user.GetClaim(
            "given_name",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
            ClaimTypes.GivenName
        );

    public static string? GetSurname(this ClaimsPrincipal user) =>
        user.GetClaim(
            "family_name",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
            ClaimTypes.Surname
        );
}
