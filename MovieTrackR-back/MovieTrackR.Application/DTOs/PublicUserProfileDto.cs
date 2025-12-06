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

    /// <summary>Nombre de critiques publiées par l'utilisateur.</summary>
    public int ReviewsCount { get; init; }

    /// <summary>Nombre de listes créées par l'utilisateur.</summary>
    public int ListsCount { get; init; }

    /// <summary>Date de création du compte.</summary>
    public DateTime CreatedAt { get; init; }
}
