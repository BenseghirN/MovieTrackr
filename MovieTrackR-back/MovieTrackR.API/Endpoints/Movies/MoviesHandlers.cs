using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Movies.Commands;
using MovieTrackR.Application.Movies.Queries;

namespace MovieTrackR.API.Endpoints.Movies;

public static class MoviesHandlers
{
    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        MovieDetailsDto? dto = await mediator.Send(new GetMovieByIdQuery(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    public static async Task<IResult> GetByTmdbId(int tmdbId, IMediator mediator, CancellationToken cancellationToken)
    {
        try
        {
            Guid localId = await mediator.Send(
                new EnsureLocalMovieCommand(MovieId: null, TmdbId: tmdbId),
                cancellationToken);

            MovieDetailsDto? dto = await mediator.Send(
                new GetMovieByIdQuery(localId),
                cancellationToken);

            return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound(new { error = $"Film TMDB {tmdbId} introuvable" });
        }
    }

    public static async Task<IResult> Search([AsParameters] MovieSearchRequest query, IMediator mediator, HttpResponse response, CancellationToken cancellationToken)
    {
        HybridPagedResult<SearchMovieResultDto> result =
        await mediator.Send(new SearchMoviesHybridQuery(query.ToCriteria()), cancellationToken);

        response.Headers["X-Total-Local"] = result.Meta.TotalLocal.ToString();
        response.Headers["X-Total-Tmdb"] = result.Meta.TotalTmdb.ToString();
        response.Headers["X-Total"] = result.Meta.TotalResults.ToString();
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetPopular([AsParameters] MovieSearchRequest query, IMediator mediator, HttpResponse response, CancellationToken cancellationToken)
    {
        HybridPagedResult<SearchMovieResultDto> result =
        await mediator.Send(new GetPopularMoviesQuery(query.ToCriteria()), cancellationToken);

        response.Headers["X-Total-Local"] = result.Meta.TotalLocal.ToString();
        response.Headers["X-Total-Tmdb"] = result.Meta.TotalTmdb.ToString();
        response.Headers["X-Total"] = result.Meta.TotalResults.ToString();
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetTrending(IMediator mediator, CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchMovieResultDto> result =
        await mediator.Send(new GetTrendingMoviesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> Create(CreateMovieDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        Guid id = await mediator.Send(new CreateMovieCommand(dto), cancellationToken);
        return TypedResults.Created($"/movies/{id}", new { id });
    }

    public static async Task<IResult> Update(Guid id, UpdateMovieDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateMovieCommand(id, dto), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteMovieCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> GetMovieStreamingOffers(int tmdbId, string? country, IMediator mediator, CancellationToken cancellationToken)
    {
        string countryCode = string.IsNullOrWhiteSpace(country)
            ? "BE"
            : country.ToUpperInvariant();
        StreamingOfferDto? dto = await mediator.Send(new GetStreamingOffersForMovieQuery(tmdbId, countryCode), cancellationToken);
        return TypedResults.Ok(dto);
    }
}