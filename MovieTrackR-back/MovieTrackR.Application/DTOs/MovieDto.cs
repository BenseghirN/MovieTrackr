namespace MovieTrackR.Application.DTOs;

/// <summary>
/// Représente un film dans la plateforme.
/// </summary>
public class MovieDto
{
    /// <summary>Identifiant unique du film dans la base locale.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifiant externe TMDb (si disponible).</summary>
    public int? TmdbId { get; set; }

    /// <summary>Titre principal du film.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Titre original du film (langue d'origine).</summary>
    public string? OriginalTitle { get; set; }

    /// <summary>Année de sortie du film (si connue).</summary>
    public int? Year { get; set; }

    /// <summary>URL vers l'affiche du film.</summary>
    public string? PosterUrl { get; set; }

    /// <summary>URL vers la bande-annonce officielle.</summary>
    public string? TrailerUrl { get; set; }

    /// <summary>Durée du film en minutes.</summary>
    public int? Duration { get; set; }

    /// <summary>Résumé ou synopsis du film.</summary>
    public string? Overview { get; set; }

    /// <summary>Date de sortie officielle (si connue).</summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>Date et heure de création de l'entrée en base.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Liste des genres associés au film.</summary>
    public IReadOnlyList<string> Genres { get; set; } = new List<string>();
}

/// <summary>
/// Données pour créer un film (création manuelle ou import préparé).
/// </summary>
public sealed record CreateMovieDto
{
    /// <summary>Titre principal du film.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Identifiant TMDb si le film provient de TMDb (optionnel).</summary>
    public int? TmdbId { get; init; }

    /// <summary>Titre original (langue d’origine).</summary>
    public string? OriginalTitle { get; init; }

    /// <summary>Année de sortie (si connue).</summary>
    public int? Year { get; init; }

    /// <summary>URL de l’affiche (si disponible).</summary>
    public string? PosterUrl { get; init; }

    /// <summary>URL de la bande-annonce (si disponible).</summary>
    public string? TrailerUrl { get; init; }

    /// <summary>Durée en minutes.</summary>
    public int? Duration { get; init; }

    /// <summary>Résumé / synopsis.</summary>
    public string? Overview { get; init; }

    /// <summary>Date de sortie officielle (si connue).</summary>
    public DateTime? ReleaseDate { get; init; }

    /// <summary>Identifiants des genres à associer au film.</summary>
    public IReadOnlyList<Guid> GenreIds { get; init; } = Array.Empty<Guid>();
}

/// <summary>
/// Données pour mettre à jour un film existant.
/// </summary>
public sealed record UpdateMovieDto
{
    /// <summary>Nouveau titre du film.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Nouveau titre original (optionnel).</summary>
    public string? OriginalTitle { get; init; }

    /// <summary>Nouvelle année de sortie (si connue).</summary>
    public int? Year { get; init; }

    /// <summary>Nouvelle URL de l’affiche (optionnelle).</summary>
    public string? PosterUrl { get; init; }

    /// <summary>Nouvelle URL de la bande-annonce (optionnelle).</summary>
    public string? TrailerUrl { get; init; }

    /// <summary>Nouvelle durée en minutes (optionnelle).</summary>
    public int? Duration { get; init; }

    /// <summary>Nouveau résumé / synopsis (optionnel).</summary>
    public string? Overview { get; init; }

    /// <summary>Nouvelle date de sortie officielle (optionnelle).</summary>
    public DateTime? ReleaseDate { get; init; }

    /// <summary>Liste complète des genres (Ids) à associer au film.</summary>
    public IReadOnlyList<Guid> GenreIds { get; init; } = Array.Empty<Guid>();
}