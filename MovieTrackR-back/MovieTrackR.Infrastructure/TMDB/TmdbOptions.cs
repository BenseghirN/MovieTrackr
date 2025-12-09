namespace MovieTrackR.Application.TMDB;

public sealed class TmdbOptions
{
    public const string SectionName = "Tmdb";

    public string? ApiKey { get; set; }
    public string? AccessTokenV4 { get; set; }
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3";
    public string MoviePosterBaseUrl = "https://image.tmdb.org/t/p/w500";
    public string ProviderIconBaseUrl = "https://image.tmdb.org/t/p/original";
    public string? DefaultLanguage { get; set; } = "fr-FR";
    public string? DefaultRegion { get; set; } = "FR";
    public int HttpTimeoutSeconds { get; set; } = 3;
    public int SearchCacheSeconds { get; set; } = 60;
}