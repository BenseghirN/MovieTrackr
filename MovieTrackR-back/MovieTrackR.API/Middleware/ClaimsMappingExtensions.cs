
using System.Security.Claims;
using MovieTrackR.Application.Common.Security;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Middleware;

public static class ClaimsMappingExtensions
{
    public static CurrentUserDto ToCurrentUserDto(this ClaimsPrincipal user)
    {
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");

        string externalId = user.GetExternalId()
            ?? throw new UnauthorizedAccessException("ExternalId introuvable dans les claims.");
        string email = user.GetEmail() ?? string.Empty;
        string displayName = user.GetDisplayName() ?? string.Empty;
        string givenName = user.GetGivenName() ?? string.Empty;
        string surname = user.GetSurname() ?? string.Empty;

        return new CurrentUserDto(
            ExternalId: externalId,
            Email: email,
            DisplayName: displayName,
            GivenName: givenName,
            Surname: surname
        );
    }

    public static CurrentUserDto? ToCurrentUserDtoOrNull(this ClaimsPrincipal user)
    {
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
            return null;
        try
        {
            return user.ToCurrentUserDto();
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}