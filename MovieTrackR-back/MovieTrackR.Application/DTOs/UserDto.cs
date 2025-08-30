using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.DTOs;

/// <summary>
/// Représente un utilisateur de la plateforme.
/// </summary>
public class UserDto
{
    /// <summary>Identifiant unique de l'utilisateur dans la base locale.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifiant externe (Azure).</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Adresse email de l'utilisateur.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Pseudo de l'utilisateur (affiché publiquement).</summary>
    public string Pseudo { get; set; } = string.Empty;

    /// <summary>Prénom de l'utilisateur (issu du provider).</summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>Nom de famille de l'utilisateur (issu du provider).</summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>Rôle de l'utilisateur dans la plateforme (User, Admin).</summary>
    public UserRole Role { get; set; } = UserRole.User;
}