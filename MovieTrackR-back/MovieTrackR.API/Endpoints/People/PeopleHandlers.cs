using MediatR;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.People.Commands;
using MovieTrackR.Application.People.Queries;

namespace MovieTrackR.API.Endpoints.People;

public static class PeopleHandlers
{
    public static async Task<IResult> Search([AsParameters] PeopleSearchRequest query, IMediator mediator, HttpResponse response, CancellationToken cancellationToken)
    {
        HybridPagedResult<SearchPersonResultDto> result =
        await mediator.Send(new SearchPeopleQuery(query.ToCriteria()), cancellationToken);

        response.Headers["X-Total-Local"] = result.Meta.TotalLocal.ToString();
        response.Headers["X-Total-Tmdb"] = result.Meta.TotalTmdb.ToString();
        response.Headers["X-Total"] = result.Meta.TotalResults.ToString();
        return Results.Ok(result);
    }

    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        PersonDetailsDto? dto = await mediator.Send(new GetPersonByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    public static async Task<IResult> GetByTmdbId(int tmdbId, IMediator mediator, CancellationToken cancellationToken)
    {
        try
        {
            Guid localId = await mediator.Send(
                new EnsureLocalPersonCommand(PersonId: null, TmdbId: tmdbId),
                cancellationToken);

            PersonDetailsDto? dto = await mediator.Send(
                new GetPersonByIdQuery(localId),
                cancellationToken);

            return dto is null ? Results.NotFound() : Results.Ok(dto);
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = $"Person TMDB {tmdbId} introuvable" });
        }
    }

    public static async Task<IResult> GetPersonMovieCredits(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        List<PersonMovieCreditDto>? dto = await mediator.Send(new GetPersonMovieCreditsQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }
}