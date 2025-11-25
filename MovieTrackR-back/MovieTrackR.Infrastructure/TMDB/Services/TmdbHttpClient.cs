using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;

namespace MovieTrackR.Application.Common.Services;

public sealed class TmdbHttpClient(HttpClient httpClient, IOptions<TmdbOptions> options, ILogger<TmdbHttpClient> logger) : ITmdbClient
{
    private readonly string? _apiKey = string.IsNullOrWhiteSpace(options.Value.AccessTokenV4)
    ? options.Value.ApiKey ?? throw new InvalidOperationException("Tmdb:ApiKey is required when AccessTokenV4 is not set.")
    : null;

    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<TmdbConfigurationImages> GetConfigurationImagesAsync(CancellationToken cancellationToken)
        => await GetFromTmdbAsync<TmdbConfigurationImages>("configuration", cancellationToken);

    public async Task<TmdbMovieCredits> GetMovieCreditsAsync(int tmdbId, CancellationToken cancellationToken)
        => await GetFromTmdbAsync<TmdbMovieCredits>($"movie/{tmdbId}/credits", cancellationToken);

    public async Task<TmdbMovieDetails> GetMovieDetailsAsync(int tmdbId, string language, CancellationToken cancellationToken)
        => await GetFromTmdbAsync<TmdbMovieDetails>($"movie/{tmdbId}?language={Uri.EscapeDataString(string.IsNullOrWhiteSpace(language) ? "fr-FR" : language)}", cancellationToken);

    public async Task<TmdbGenresResponse> GetGenresAsync(string language = "fr-FR", CancellationToken cancellationToken = default)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string relative = $"/genre/movie/list?language={Uri.EscapeDataString(lang)}";
        return await GetFromTmdbAsync<TmdbGenresResponse>(relative, cancellationToken);
    }

    public async Task<TmdbSearchMoviesResponse> SearchMoviesAsync(MovieSearchCriteria criterias, string language, string? region, CancellationToken cancellationToken)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string q = string.IsNullOrWhiteSpace(criterias.Query) ? string.Empty : Uri.EscapeDataString(criterias.Query);
        int safePage = criterias.Page <= 0 ? 1 : criterias.Page;

        string relative = $"search/movie?query={q}&page={safePage}&language={Uri.EscapeDataString(lang)}&include_adult=false";
        if (criterias.Year.HasValue)
        {
            relative += $"&year={criterias.Year.Value}";
        }
        if (!string.IsNullOrWhiteSpace(region))
            relative += $"&region={Uri.EscapeDataString(region)}";

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