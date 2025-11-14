namespace MovieTrackR.Application.TMDB.Interfaces;

public interface ITmdbCatalogService
{
    /// <summary>Seed minimal du film (si absent), puis enqueue enrichissement (async) ; retourne l'Id local.</summary>
    Task<Guid> ImportMovieAsync(int tmdbId, CancellationToken ct);

    /// <summary>Complète un film local (détails, genres, cast/crew) à partir de son Id local.</summary>
    Task EnrichMovieAsync(Guid movieId, CancellationToken ct);
}