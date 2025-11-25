using MovieTrackR.Application.Movies;

namespace MovieTrackR.Application.DTOs;

/// <summary>Paramètres de recherche d'un film.</summary>
public sealed class MovieSearchRequest
{
    /// <summary>Texte recherché (titre/originalTitle).</summary>
    public string? Query { get; init; }

    /// <summary>Filtre par année exacte.</summary>
    public int? Year { get; init; }

    /// <summary>Numéro de page (>= 1).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Taille de page (1..100).</summary>
    public int PageSize { get; init; } = 20;

    public MovieSearchCriteria ToCriteria() => new()
    {
        Query = Query?.Trim(),
        Year = Year,
        Page = Page,
        PageSize = PageSize
    };
}
