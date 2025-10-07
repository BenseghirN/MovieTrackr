
using System.Security.Claims;
using MovieTrackR.Application.Common.Security;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Middleware;

public static class ClaimsMappingExtensions
{
    public static CurrentUserDto ToCurrentUserDto(this ClaimsPrincipal user)
    {
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
}