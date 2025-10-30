using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;

namespace MovieTrackR.Application.Common.Services;

public sealed class TmdbHttpClient(HttpClient httpClient, IOptions<TmdbOptions> options, ILogger<TmdbHttpClient> logger) : ITmdbClient
{
    private readonly string _apiKey = string.IsNullOrWhiteSpace(options.Value.ApiKey)
        ? throw new InvalidOperationException("Tmdb:ApiKey is required.")
        : options.Value.ApiKey!;

    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public Task<TmdbConfigurationImages> GetConfigurationImagesAsync(CancellationToken cancellationToken)
        => GetFromTmdbAsync<TmdbConfigurationImages>("configuration", cancellationToken);

    public Task<TmdbMovieCredits> GetMovieCreditsAsync(int tmdbId, CancellationToken cancellationToken)
        => GetFromTmdbAsync<TmdbMovieCredits>($"movie/{tmdbId}/credits", cancellationToken);

    public Task<TmdbMovieDetails> GetMovieDetailsAsync(int tmdbId, string language, CancellationToken cancellationToken)
        => GetFromTmdbAsync<TmdbMovieDetails>($"movie/{tmdbId}?language={Uri.EscapeDataString(string.IsNullOrWhiteSpace(language) ? "fr-FR" : language)}", cancellationToken);

    public Task<TmdbSearchMoviesResponse> SearchMoviesAsync(string query, int page, string language, string? region, CancellationToken ct)
    {
        string lang = string.IsNullOrWhiteSpace(language) ? "fr-FR" : language;
        string q = string.IsNullOrWhiteSpace(query) ? string.Empty : Uri.EscapeDataString(query);
        int safePage = page <= 0 ? 1 : page;

        string relative = $"search/movie?query={q}&page={safePage}&language={Uri.EscapeDataString(lang)}&include_adult=false";
        if (!string.IsNullOrWhiteSpace(region))
            relative += $"&region={Uri.EscapeDataString(region)}";

        return GetFromTmdbAsync<TmdbSearchMoviesResponse>(relative, ct);
    }

    private async Task<T> GetFromTmdbAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        string sep = relativeUrl.Contains('?') ? "&" : "?";
        string urlWithKey = $"{relativeUrl}{sep}api_key={Uri.EscapeDataString(_apiKey)}";

        using HttpResponseMessage res = await httpClient.GetAsync(urlWithKey, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            string body = await res.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("TMDb call failed: {Status} {Url} {Body}", (int)res.StatusCode, urlWithKey, body);
            res.EnsureSuccessStatusCode();
        }

        using Stream stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        T? data = await JsonSerializer.DeserializeAsync<T>(stream, _json, cancellationToken);
        if (data is null) throw new InvalidOperationException($"TMDb returned empty payload for '{relativeUrl}'.");

        return data;
    }
}