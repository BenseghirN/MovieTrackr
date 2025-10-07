namespace MovieTrackR.Application.DTOs;

/// <summary>
/// Contexte minimal de l'utilisateur courant, transmis depuis l'API vers l'Application
/// sans exposer <c>ClaimsPrincipal</c>.
/// </summary>
/// <param name="ExternalId">Identifiant externe (ex: sub/oid) – requis.</param>
/// <param name="Email">Email si disponible.</param>
/// <param name="DisplayName">Nom d'affichage si disponible.</param>
/// <param name="GivenName">Prénom si disponible.</param>
/// <param name="Surname">Nom si disponible.</param>
public sealed record CurrentUserDto(
    string ExternalId,
    string? Email,
    string? DisplayName,
    string? GivenName,
    string? Surname
);
