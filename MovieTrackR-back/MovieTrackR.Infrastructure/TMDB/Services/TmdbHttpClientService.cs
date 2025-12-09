using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;

namespace MovieTrackR.Application.Common.Services;

public sealed class TmdbHttpClientService(HttpClient httpClient, IOptions<TmdbOptions> options, ILogger<TmdbHttpClientService> logger) : ITmdbClientService
{
    private readonly string? _apiKey = string.IsNullOrWhiteSpace(options.Value.AccessTokenV4)
    ? options.Value.ApiKey ?? throw new InvalidOperationException("Tmdb:ApiKey is required when AccessTokenV4 is not set.")
    : null;

    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public async Task<TmdbWatchProvidersResponse?> GetMovieWatchProvidersAsync(int tmdbId, CancellationToken cancellationToken)
    {
        string relative = $"movie/{tmdbId}/watch/providers";
        return await GetFromTmdbAsync<TmdbWatchProvidersResponse>(relative, cancellationToken);
    }

    public async Task<TmdbConfigurationImages> GetConfigurationImagesAsync(CancellationToken cancellationToken)
        => await GetFromTmdbAsync<TmdbConfigurationImages>("configuration", cancellationToken);

    public async Task<TmdbMovieCredits> GetMovieCreditsAsync(int tmdbId, string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"movie/{tmdbId}/credits?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbMovieCredits>(relative, cancellationToken);
    }

    public async Task<TmdbMovieDetails> GetMovieDetailsAsync(int tmdbId, string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"movie/{tmdbId}?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbMovieDetails>(relative, cancellationToken);
    }

    public async Task<TmdbVideosResponse> GetMovieVideosAsync(int tmdbId, string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"movie/{tmdbId}/videos?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbVideosResponse>(relative, cancellationToken);
    }

    public async Task<TmdbGenresResponse> GetGenresAsync(string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"/genre/movie/list?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbGenresResponse>(relative, cancellationToken);
    }

    public async Task<TmdbSearchMoviesResponse> SearchMoviesAsync(MovieSearchCriteria criterias, string language, CancellationToken cancellationToken)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string q = string.IsNullOrWhiteSpace(criterias.Query) ? string.Empty : Uri.EscapeDataString(criterias.Query);
        int safePage = criterias.Page <= 0 ? 1 : criterias.Page;

        string relative = $"search/movie?query={q}&page={safePage}&language={Uri.EscapeDataString(lang)}&include_adult=false";
        if (criterias.Year.HasValue)
        {
            relative += $"&year={criterias.Year.Value}";
        }
        return await GetFromTmdbAsync<TmdbSearchMoviesResponse>(relative, cancellationToken);
    }

    public async Task<TmdbSearchPeopleResponse> SearchPeopleAsync(string query, int page, string? language, CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        int safePage = page <= 0 ? 1 : page;
        string q = Uri.EscapeDataString(query);
        string relative = $"search/person?query={q}&page={safePage}&language={Uri.EscapeDataString(lang)}&include_adult=false";
        return await GetFromTmdbAsync<TmdbSearchPeopleResponse>(relative, cancellationToken);
    }

    public async Task<TmdbPersonDetails?> GetPersonDetailsAsync(int tmdbId, string? language, CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"person/{tmdbId}?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbPersonDetails>(relative, cancellationToken);
    }

    public async Task<TmdbPersonMovieCredits?> GetPersonMovieCreditsAsync(int tmdbId, string? language, CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"person/{tmdbId}/movie_credits?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbPersonMovieCredits>(relative, cancellationToken);
    }

    public async Task<TmdbSearchMoviesResponse> GetPopularMovies(string? language, int page, CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        int safePage = page <= 0 ? 1 : page;
        string relative = $"movie/popular?&page={safePage}&language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbSearchMoviesResponse>(relative, cancellationToken);
    }

    private async Task<T> GetFromTmdbAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        string url = relativeUrl.StartsWith("/") ? relativeUrl[1..] : relativeUrl;
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            string sep = url.Contains('?') ? "&" : "?";
            url = $"{url}{sep}api_key={Uri.EscapeDataString(_apiKey)}";
        }

        logger.LogInformation("TMDb GET {Base}{Url}", httpClient.BaseAddress, url);

        using var res = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            string body = await res.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("TMDb failed: {Status} {Url} {Body}", (int)res.StatusCode, url, body);
            res.EnsureSuccessStatusCode();
        }

        await using Stream stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        T? data = await JsonSerializer.DeserializeAsync<T>(stream, _json, cancellationToken);
        return data ?? throw new InvalidOperationException($"Empty TMDb payload for '{relativeUrl}'.");
    }
}