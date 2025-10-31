namespace MovieTrackR.Application.DTOs;

public sealed record PageMeta(
    int Page,          // page courante (1-based)
    int PageSize,      // taille de page demandée
    int? TotalPages,   // total de pages TMDB (si dispo)
    int? TotalResults, // total TMDB brut (si dispo)
    int TotalLocal,    // total local exact
    bool HasMore       // reste-t-il des résultats ?
);

public sealed record HybridPagedResult<T>(
    IReadOnlyList<T> Items,
    PageMeta Meta
);
