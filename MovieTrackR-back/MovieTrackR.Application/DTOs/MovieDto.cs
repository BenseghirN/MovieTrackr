namespace MovieTrackR.Application.DTOs;

/// <summary>
/// Représente un film dans la plateforme.
/// </summary>
public class MovieDetailsDto
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

    /// <summary>URL vers l’image de fond du film.</summary>
    public string? BackdropPath { get; set; }

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

    /// <summary>Vote average du film.</summary>
    public double? VoteAverage { get; set; }

    /// <summary>Liste des genres associés au film.</summary>
    public IReadOnlyList<GenreDto> Genres { get; set; } = [];

    /// <summary>Liste des acteurs principaux du film.</summary>
    public IReadOnlyList<CastMemberDto> Cast { get; set; } = [];

    /// <summary>Liste des membres de l'équipe du film.</summary>
    public IReadOnlyList<CrewMemberDto> Crew { get; set; } = [];
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

    /// <summary>URL de l’image de fond (si disponible).</summary>
    public string? BackdropPath { get; init; }

    /// <summary>URL de la bande-annonce (si disponible).</summary>
    public string? TrailerUrl { get; init; }

    /// <summary>Durée en minutes.</summary>
    public int? Duration { get; init; }

    /// <summary>Résumé / synopsis.</summary>
    public string? Overview { get; init; }

    /// <summary>Date de sortie officielle (si connue).</summary>
    public DateTime? ReleaseDate { get; init; }

    /// <summary>Vote average du film.</summary>
    public double? VoteAverage { get; init; }

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

    /// <summary>Nouvelle URL de l’image de fond (optionnelle).</summary>
    public string? BackdropPath { get; init; }

    /// <summary>Nouvelle URL de la bande-annonce (optionnelle).</summary>
    public string? TrailerUrl { get; init; }

    /// <summary>Nouvelle durée en minutes (optionnelle).</summary>
    public int? Duration { get; init; }

    /// <summary>Nouveau résumé / synopsis (optionnel).</summary>
    public string? Overview { get; init; }

    /// <summary>Nouveau vote average (optionnel).</summary>
    public double? VoteAverage { get; init; }

    /// <summary>Nouvelle date de sortie officielle (optionnelle).</summary>
    public DateTime? ReleaseDate { get; init; }

    /// <summary>Liste complète des genres (Ids) à associer au film.</summary>
    public IReadOnlyList<Guid> GenreIds { get; init; } = Array.Empty<Guid>();
}

/// <summary>Membre de la distribution.</summary>
public sealed class CastMemberDto
{
    /// <summary>Nom de l'acteur/actrice.</summary>
    public required string Name { get; init; }
    /// <summary>Nom du personnage interprété.</summary>
    public string? Character { get; init; }
    /// <summary>Chemin vers l'image de profil.</summary>
    public string? ProfilePath { get; init; }
    /// <summary>Ordre de crédit dans le casting.</summary>
    public int? Order { get; init; }
}

/// <summary>Membre de l'équipe technique.</summary>
public sealed class CrewMemberDto
{
    /// <summary>Nom du membre de l'équipe.</summary>
    public required string Name { get; init; }
    /// <summary>Rôle ou fonction du membre dans l'équipe.</summary>
    public required string Job { get; init; }
    /// <summary>Département du membre dans la production.</summary>
    public string? Department { get; init; }
    /// <summary>Chemin vers l'image de profil.</summary>
    public string? ProfilePath { get; init; }
}