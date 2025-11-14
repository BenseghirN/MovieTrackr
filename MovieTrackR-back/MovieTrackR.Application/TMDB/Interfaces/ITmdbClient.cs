namespace MovieTrackR.Application.TMDB.Interfaces;

/// <summary>
/// Client en lecture seule vers TMDb (contrat pur, sans dépendance HTTP).
/// </summary>
public interface ITmdbClient
{
    Task<TmdbSearchMoviesResponse> SearchMoviesAsync(
        string query, int page, string language, string? region, CancellationToken cancellationToken);

    Task<TmdbMovieDetails> GetMovieDetailsAsync(
        int tmdbId, string language, CancellationToken cancellationToken);

    Task<TmdbMovieCredits> GetMovieCreditsAsync(
        int tmdbId, CancellationToken cancellationToken);

    Task<TmdbConfigurationImages> GetConfigurationImagesAsync(CancellationToken cancellationToken);
}