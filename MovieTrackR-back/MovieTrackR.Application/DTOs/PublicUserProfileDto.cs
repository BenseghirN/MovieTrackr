namespace MovieTrackR.Application.DTOs;


/// <summary>
/// Représente les informations publiques du profil d'un utilisateur.
/// </summary>
public sealed class PublicUserProfileDto
{
    /// <summary>Identifiant unique de l'utilisateur.</summary>
    public Guid Id { get; init; }

    /// <summary>Pseudo public de l'utilisateur.</summary>
    public string Pseudo { get; init; } = string.Empty;

    /// <summary>Adresse email de l'utilisateur.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Prénom de l'utilisateur (issu du provider).</summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>Nom de famille de l'utilisateur (issu du provider).</summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>URL de l'avatar de l'utilisateur</summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>Nombre de critiques publiées par l'utilisateur.</summary>
    public int ReviewsCount { get; init; }

    /// <summary>Nombre de listes créées par l'utilisateur.</summary>
    public int ListsCount { get; init; }

    /// <summary>Date de création du compte.</summary>
    public DateTime CreatedAt { get; init; }
}
