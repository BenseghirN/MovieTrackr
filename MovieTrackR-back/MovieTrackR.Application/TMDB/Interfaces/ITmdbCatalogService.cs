using MovieTrackR.Application.DTOs;

namespace MovieTrackR.Application.TMDB.Interfaces;

public interface ITmdbCatalogService
{
    /// <summary>Seed minimal du film (si absent), puis enqueue enrichissement (async) ; retourne l'Id local.</summary>
    Task<Guid> ImportMovieAsync(int tmdbId, CancellationToken cancellationToken);
    /// <summary>Complète un film local (détails, genres, cast/crew) à partir de son Id local.</summary>
    Task EnrichMovieAsync(Guid movieId, CancellationToken cancellationToken);
    /// <summary>Récupère la liste des plateformes et offres de streaming d'un film depuis TMDB.</summary> 
    Task<StreamingOfferDto?> GetMovieStreamingOffersAsync(int tmdbId, string countryCode = "BE", CancellationToken cancellationToken = default);
}