using MovieTrackR.Application.People;

namespace MovieTrackR.Application.DTOs;

/// <summary>Paramètres de recherche d'une personne.</summary>
public sealed class PeopleSearchRequest
{
    /// <summary>Texte recherché (nom).</summary>
    public string? Query { get; init; }

    /// <summary>Numéro de page (>= 1).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Taille de page (1..100).</summary>
    public int PageSize { get; init; } = 20;

    public PeopleSearchCriteria ToCriteria() => new()
    {
        Query = Query?.Trim(),
        Page = Page,
        PageSize = PageSize
    };
}
