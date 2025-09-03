using MovieTrackR.Application.Movies;

namespace MovieTrackR.Application.DTOs;

/// <summary>Paramètres de recherche d'un film.</summary>
public sealed class MovieSearchRequest
{
    /// <summary>Texte recherché (titre/originalTitle).</summary>
    public string? Search { get; init; }

    /// <summary>Filtre par année exacte.</summary>
    public int? Year { get; init; }

    /// <summary>Filtre par genre (Id).</summary>
    public Guid? GenreId { get; init; }

    /// <summary>Numéro de page (>= 1).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Taille de page (1..100).</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Tri: "title" ou "year".</summary>
    public string? Sort { get; init; }

    public MovieSearchCriteria ToCriteria() => new()
    {
        Search = Search?.Trim(),
        Year = Year,
        GenreId = GenreId,
        Page = Page,
        PageSize = PageSize,
        Sort = Sort
    };
}
