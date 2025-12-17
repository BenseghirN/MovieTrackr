using System.ComponentModel;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.SemanticKernel;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Genres.Queries;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.Movies.Queries;

namespace MovieTrackR.AI.Agents.ActorSeekerAgent.Plugins;

public sealed class DiscoverMoviesPlugin(IMediator mediator)
{
    [KernelFunction("discover_movies")]
    [Description(
        "Discover movies by STRICT filters: release year + one or more genreIds (TMDB genre ids stored in MovieTrackR DB). " +
        "This is a hybrid query (local DB + TMDB discover). " +
        "Do NOT call this without having validated genreIds from the database. " +
        "Use page >= 1. Returns a paginated result with up to PageSize items (default 20) and meta info."
    )]
    public async Task<HybridPagedResult<SearchMovieResultDto>> DiscoverMoviesAsync(
        [Description("Release year to filter movies. Must be a 4-digit year (e.g., 1900..2100).")] int year,
        [Description(
            "List of TMDB genre ids (integers) as stored in MovieTrackR database (Genre.TmdbId). " +
            "Example: [28, 12] for Action + Adventure. " +
            "Never invent ids: resolve them via find_genre_by_name or get_all_genres."
        )] List<int> genreIds,
        [Description("Page number (1-based). Example: 1 for first page, 2 for next page.")] int page,
        CancellationToken cancellationToken = default)
    {
        DiscoverCriteria criterias = new DiscoverCriteria
        {
            Year = year,
            GenreIds = genreIds,
            Page = page
        };
        return await mediator.Send(new DiscoverMoviesQuery(criterias), cancellationToken);
    }

    [KernelFunction("get_all_genres")]
    [Description(
        "Return the full list of movie genres available in MovieTrackR (local database), including their TMDB ids. " +
        "Use this when a user provides a genre name that cannot be resolved directly, to propose the closest matches."
    )]
    public async Task<IReadOnlyList<GenreDto>> GetAllGenresAsync(
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new GetAllGenresQuery(), cancellationToken);
    }

    [KernelFunction("find_genre_by_name")]
    [Description(
        "Find a single genre in MovieTrackR by a human name (case-insensitive). " +
        "Example inputs: 'Science-Fiction', 'Action', 'Comédie'. " +
        "Returns null if no close match exists. " +
        "If null, call get_all_genres and ask the user to pick among suggestions."
    )]
    public async Task<GenreDto?> FindGenreByNameAsync(
        [Description("Genre name provided by the user. Example: 'Science-Fiction' Do NOT pass ids here, only text."
        )] string genreName,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new FindGenreByNameQuery(genreName), cancellationToken);
    }
}

public sealed class DiscoverMovieCandidateAttachment
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("localId")]
    public Guid? LocalId { get; set; }

    [JsonPropertyName("tmdbId")]
    public int? TmdbId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("posterPath")]
    public string? PosterPath { get; set; }
}