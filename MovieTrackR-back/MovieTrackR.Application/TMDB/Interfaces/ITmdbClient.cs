using MovieTrackR.Application.Movies;

namespace MovieTrackR.Application.TMDB.Interfaces;

/// <summary>
/// Client en lecture seule vers TMDb (contrat pur, sans dépendance HTTP).
/// </summary>
public interface ITmdbClientService
{
    Task<TmdbSearchMoviesResponse> SearchMoviesAsync(MovieSearchCriteria criterias, string language, CancellationToken cancellationToken);
    Task<TmdbSearchMoviesResponse> SearchSimilarMoviesAsync(int tmdbMovieId, string language, CancellationToken cancellationToken);
    Task<TmdbMovieDetails> GetMovieDetailsAsync(int tmdbId, string language, CancellationToken cancellationToken);
    Task<TmdbMovieCredits> GetMovieCreditsAsync(int tmdbId, string language, CancellationToken cancellationToken);
    Task<TmdbVideosResponse> GetMovieVideosAsync(int tmdbId, string language, CancellationToken cancellationToken);
    Task<TmdbWatchProvidersResponse?> GetMovieWatchProvidersAsync(int tmdbId, CancellationToken cancellationToken);
    Task<TmdbConfigurationImages> GetConfigurationImagesAsync(CancellationToken cancellationToken);
    Task<TmdbGenresResponse> GetGenresAsync(string language, CancellationToken cancellationToken);
    Task<TmdbSearchPeopleResponse> SearchPeopleAsync(string query, int page, string? language, CancellationToken cancellationToken = default);
    Task<TmdbPersonDetails?> GetPersonDetailsAsync(int tmdbId, string? language, CancellationToken cancellationToken = default);
    Task<TmdbPersonMovieCredits?> GetPersonMovieCreditsAsync(int tmdbId, string? language, CancellationToken cancellationToken = default);
    Task<TmdbSearchMoviesResponse> GetPopularMovies(string? language, int page, CancellationToken cancellationToken = default);
}