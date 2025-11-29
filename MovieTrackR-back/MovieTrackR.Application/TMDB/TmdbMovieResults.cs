using System.Collections.Specialized;
using System.Text.Json.Serialization;

namespace MovieTrackR.Application.TMDB;

public sealed record TmdbSearchMoviesResponse(
    [property: JsonPropertyName("page")]
    int Page,
    [property: JsonPropertyName("total_results")]
    int TotalResults,
    [property: JsonPropertyName("total_pages")]
    int TotalPages,
    [property: JsonPropertyName("results")]
    IReadOnlyList<TmdbSearchMovieItem> Results
);

public sealed record TmdbSearchMovieItem(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("title")]
    string? Title,
    [property: JsonPropertyName("original_title")]
    string? OriginalTitle,
    [property: JsonPropertyName("release_date")]
    string? ReleaseDate,
    [property: JsonPropertyName("poster_path")]
    string? PosterPath,
    [property: JsonPropertyName("vote_average")]
    double? VoteAverage,
    [property: JsonPropertyName("popularity")]
    double? Popularity
);

public sealed record TmdbMovieDetails(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("title")]
    string? Title,
    [property: JsonPropertyName("original_title")]
    string? OriginalTitle,
    [property: JsonPropertyName("original_language")]
    string? OriginalLanguage,
    [property: JsonPropertyName("overview")]
    string? Overview,
    [property: JsonPropertyName("release_date")]
    string? ReleaseDate,
    [property: JsonPropertyName("poster_path")]
    string? PosterPath,
    [property: JsonPropertyName("backdrop_path")]
    string? BackdropPath,
    [property: JsonPropertyName("runtime")]
    int? Runtime,
    [property: JsonPropertyName("vote_average")]
    double? VoteAverage,
    [property: JsonPropertyName("vote_count")]
    int? VoteCount,
    [property: JsonPropertyName("genres")]
    IReadOnlyList<TmdbGenre> Genres
);

public sealed record TmdbGenre(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("name")]
    string Name
);
public sealed record TmdbGenresResponse(
    [property: JsonPropertyName("genres")]
    IReadOnlyList<TmdbGenre> Genres
);

public sealed record TmdbMovieCredits(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("cast")]
    IReadOnlyList<TmdbCast> Cast,
    [property: JsonPropertyName("crew")]
    IReadOnlyList<TmdbCrew> Crew
);

public sealed record TmdbCast(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("name")]
    string? Name,
    [property: JsonPropertyName("character")]
    string? Character,
    [property: JsonPropertyName("order")]
    int? Order,
    [property: JsonPropertyName("profile_path")]
    string? ProfilePath
);

public sealed record TmdbCrew(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("name")]
    string? Name,
    [property: JsonPropertyName("department")]
    string? Department,
    [property: JsonPropertyName("job")]
    string? Job,
    [property: JsonPropertyName("profile_path")]
    string? ProfilePath
);

public sealed record TmdbConfigurationImages(
    [property: JsonPropertyName("base_url")]
    string BaseUrl,
    [property: JsonPropertyName("secure_base_url")]
    string SecureBaseUrl,
    [property: JsonPropertyName("poster_sizes")]
    IReadOnlyList<string> PosterSizes,
    [property: JsonPropertyName("backdrop_sizes")]
    IReadOnlyList<string> BackdropSizes,
    [property: JsonPropertyName("profile_sizes")]
    IReadOnlyList<string> ProfileSizes
);

public sealed record TmdbMovieVideo(
     [property: JsonPropertyName("name")]
     string Name,
     [property: JsonPropertyName("key")]
     string Key,
     [property: JsonPropertyName("site")]
     string Site,
     [property: JsonPropertyName("type")]
     string Type,
     [property: JsonPropertyName("official")]
     bool Official
);

public sealed record TmdbVideosResponse(
    [property: JsonPropertyName("results")]
    IReadOnlyList<TmdbMovieVideo> Results
);