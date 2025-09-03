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

public sealed record CreateMovieDto(
    string Title,
    int? TmdbId,
    string? OriginalTitle,
    int? Year,
    string? PosterUrl,
    string? TrailerUrl,
    int? Duration,
    string? Overview,
    DateTime? ReleaseDate,
    IReadOnlyList<Guid> GenreIds);

public sealed record UpdateMovieDto(
    string Title,
    string? OriginalTitle,
    int? Year,
    string? PosterUrl,
    string? TrailerUrl,
    int? Duration,
    string? Overview,
    DateTime? ReleaseDate,
    IReadOnlyList<Guid> GenreIds);