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
    [property: JsonPropertyName("tagline")]
    string? Tagline,
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

public sealed record TmdbWatchProvidersResponse(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("results")]
    Dictionary<string, TmdbWatchProviderCountry> Results
);

public sealed record TmdbWatchProviderCountry(
    [property: JsonPropertyName("link")]
    string Link,
    [property: JsonPropertyName("flatrate")]
    IReadOnlyList<TmdbProvider>? Flatrate,
    [property: JsonPropertyName("free")]
    IReadOnlyList<TmdbProvider>? Free
);

public sealed record TmdbProvider(
    [property: JsonPropertyName("logo_path")]
    string? LogoPath,
    [property: JsonPropertyName("provider_id")]
    int ProviderId,
    [property: JsonPropertyName("provider_name")]
    string ProviderName,
    [property: JsonPropertyName("display_priority")]
    int DisplayPriority
);

public sealed record TmdbSearchPeopleResponse(
    [property: JsonPropertyName("page")]
    int Page,
    [property: JsonPropertyName("total_results")]
    int TotalResults,
    [property: JsonPropertyName("total_pages")]
    int TotalPages,
    [property: JsonPropertyName("results")]
    IReadOnlyList<TmdbPerson> Results
);

public sealed record TmdbPerson(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("original_name")]
    string OriginalName,
    [property: JsonPropertyName("profile_path")]
    string? ProfilePath,
    [property: JsonPropertyName("known_for_department")]
    string? KnownForDepartment,
    [property: JsonPropertyName("popularity")]
    double Popularity,
    [property: JsonPropertyName("adult")]
    bool Adult,
    [property: JsonPropertyName("gender")]
    int Gender
);

public sealed record TmdbPersonDetails(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("profile_path")]
    string? ProfilePath,
    [property: JsonPropertyName("biography")]
    string? Biography,
    [property: JsonPropertyName("birthday")]
    string? Birthday,
    [property: JsonPropertyName("deathday")]
    string? Deathday,
    [property: JsonPropertyName("place_of_birth")]
    string? PlaceOfBirth,
    [property: JsonPropertyName("known_for_department")]
    string? KnownForDepartment,
    [property: JsonPropertyName("popularity")]
    double Popularity,
    [property: JsonPropertyName("adult")]
    bool Adult,
    [property: JsonPropertyName("gender")]
    int Gender
);

// Crédits films d'une personne
public sealed record TmdbPersonMovieCredits(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("cast")]
    IReadOnlyList<TmdbPersonCastCredit> Cast,
    [property: JsonPropertyName("crew")]
    IReadOnlyList<TmdbPersonCrewCredit> Crew
);

// Film dans lequel la personne a joué (cast)
public sealed record TmdbPersonCastCredit(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("title")]
    string Title,
    [property: JsonPropertyName("poster_path")]
    string? PosterPath,
    [property: JsonPropertyName("release_date")]
    string? ReleaseDate,
    [property: JsonPropertyName("character")]
    string? Character,
    [property: JsonPropertyName("order")]
    int Order
);

// Film dans lequel la personne a travaillé (crew)
public sealed record TmdbPersonCrewCredit(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("title")]
    string Title,
    [property: JsonPropertyName("poster_path")]
    string? PosterPath,
    [property: JsonPropertyName("release_date")]
    string? ReleaseDate,
    [property: JsonPropertyName("job")]
    string? Job,
    [property: JsonPropertyName("department")]
    string? Department
);