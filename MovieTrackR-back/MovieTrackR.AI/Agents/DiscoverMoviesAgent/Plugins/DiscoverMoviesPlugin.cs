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
    [Description("")]
    public async Task<HybridPagedResult<SearchMovieResultDto>> DiscoverMoviesAsync(
        [Description("")] int year,
        [Description("")] List<int> genreIds,
        [Description("")] int page,
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
    [Description("")]
    public async Task<IReadOnlyList<GenreDto>> GetAllGenresAsync(
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new GetAllGenresQuery(), cancellationToken);
    }

    [KernelFunction("find_genre_by_name")]
    [Description("")]
    public async Task<GenreDto?> GetAllGenresAsync(
        [Description("")] string genreName,
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