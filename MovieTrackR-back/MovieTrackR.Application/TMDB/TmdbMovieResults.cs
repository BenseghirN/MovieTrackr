namespace MovieTrackR.Application.TMDB;

public sealed record TmdbSearchMoviesResponse(
    int Page,
    int TotalResults,
    int TotalPages,
    IReadOnlyList<TmdbSearchMovieItem> Results
);

public sealed record TmdbSearchMovieItem(
    int Id,
    string? Title,
    string? OriginalTitle,
    string? ReleaseDate,
    string? PosterPath,
    double? VoteAverage,
    double? Popularity
);

public sealed record TmdbMovieDetails(
    int Id,
    string? Title,
    string? OriginalTitle,
    string? OriginalLanguage,
    string? Overview,
    string? ReleaseDate,
    string? PosterPath,
    string? BackdropPath,
    int? Runtime,
    double? VoteAverage,
    int? VoteCount,
    IReadOnlyList<TmdbGenre> Genres
);

public sealed record TmdbGenre(int Id, string Name);

public sealed record TmdbMovieCredits(
    int Id,
    IReadOnlyList<TmdbCast> Cast,
    IReadOnlyList<TmdbCrew> Crew
);

public sealed record TmdbCast(
    int Id,
    string? Name,
    string? Character,
    int? Order,
    string? ProfilePath
);

public sealed record TmdbCrew(
    int Id,
    string? Name,
    string? Department,
    string? Job,
    string? ProfilePath
);

public sealed record TmdbConfigurationImages(
    string BaseUrl,
    string SecureBaseUrl,
    IReadOnlyList<string> PosterSizes,
    IReadOnlyList<string> BackdropSizes,
    IReadOnlyList<string> ProfileSizes
);
