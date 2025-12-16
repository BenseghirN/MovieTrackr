using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.Movies.Commands;
using MovieTrackR.Application.Movies.Queries;

namespace MovieTrackR.AI.Agents.ActorSeekerAgent.Plugins;

public sealed class SimilarMovieSeekerPlugin(IMediator mediator)
{

    [KernelFunction("search_movies")]
    [Description("Search movies in MovieTrackR by name (hybrid: local + TMDB). Returns a paginated list of matches.")]
    public async Task<HybridPagedResult<SearchMovieResultDto>> SearchMoviesAsync(
        [Description("Movie title or partial title to search for (e.g. 'Interstellar', 'John Wick')")] string query,
        [Description("Page number (1-based). Default is 1.")] int page = 1,
        [Description("How many results to return per page (1-10). Default is 5.")] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new HybridPagedResult<SearchMovieResultDto>(
                Array.Empty<SearchMovieResultDto>(),
                new PageMeta(Page: 1, PageSize: 20, TotalLocal: 0, TotalTmdb: 0, TotalResults: 0, TotalTmdbPages: null, HasMore: false)
            );

        MovieSearchCriteria criteria = new MovieSearchCriteria
        {
            Query = query.Trim(),
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 10)
        };

        return await mediator.Send(new SearchMoviesHybridQuery(criteria), cancellationToken);
    }

    [KernelFunction("get_movie_details")]
    [Description("Get detailed information about a specific movie using its local ID or TMDB ID")]
    [return: Description("Detailed movie information including cast, crew, genres, and ratings")]
    public async Task<MovieDetailsDto?> GetMovieByDetailsAsync(
        [Description("Local database UUID of the movie (e.g. 'a2b8dcdc-cac4-11f0-91b8-cb53db0d0acc')")] Guid? localMovieId,
        [Description("TMDB ID of the movie (e.g. 245891 for John Wick)")] int? tmdbMovieId,
        CancellationToken cancellationToken = default)
    {
        if (localMovieId is null && tmdbMovieId is null)
            throw new ArgumentException("Provide either localMovieId or tmdbMovieId.");

        Guid localId;

        if (localMovieId is not null)
        {
            localId = localMovieId.Value;
        }
        else
        {
            localId = await mediator.Send(
                new EnsureLocalMovieCommand(MovieId: null, TmdbId: tmdbMovieId),
                cancellationToken);
        }

        return await mediator.Send(new GetMovieByIdQuery(localId), cancellationToken);
    }

    [KernelFunction("search_similar_movies")]
    [Description("Find similar movies using hybrid search (local intelligent scoring + TMDB recommendations). Returns up to 10 results: 5 from local database with smart scoring (same director, actors, genres) + 5 from TMDB API.")]
    public async Task<IReadOnlyList<SearchMovieResultDto>> SearchSimilarMoviesAsync(
    [Description("TMDB ID of the reference movie (e.g. 27205 for Inception, 245891 for John Wick)")] int tmdbMovieId,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new SearchSimilarMoviesQuery(tmdbMovieId), cancellationToken);
    }
}

public sealed class MovieCandidateAttachment
{
    public int Index { get; init; }              // 1..N (utile pour "le 2")
    public Guid? LocalId { get; init; }
    public int? TmdbId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int? Year { get; init; }
    public string? PosterPath { get; init; }
}