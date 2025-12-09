namespace MovieTrackR.Application.DTOs;

/// <summary>
/// Représente un genre de film.
/// </summary>
public class GenreDto
{
    /// <summary>Identifiant unique du genre.</summary>
    public required Guid Id { get; init; }

    /// <summary>Nom du genre (ex: "Action", "Comédie", "Drame").</summary>
    public required string Name { get; init; }

    /// <summary>Identifiant TMDB du genre, si synchronisé avec TMDB.</summary>
    public int? TmdbId { get; init; }
}