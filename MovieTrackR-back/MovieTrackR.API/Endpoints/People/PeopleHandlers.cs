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
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        PersonDetailsDto? dto = await mediator.Send(new GetPersonByIdQuery(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    public static async Task<IResult> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        IReadOnlyList<PersonDetailsDto> dto = await mediator.Send(new GetAllPeopleQuery(), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
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

            return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound(new { error = $"Person TMDB {tmdbId} introuvable" });
        }
    }

    public static async Task<IResult> GetPersonMovieCredits(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        List<PersonMovieCreditDto>? dto = await mediator.Send(new GetPersonMovieCreditsQuery(id), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<IResult> Create(CreatePersonDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        Guid id = await mediator.Send(new CreatePersonCommand(dto), cancellationToken);
        return TypedResults.Created($"/people/{id}", new { id });
    }

    public static async Task<IResult> Update(Guid id, UpdatePersonDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdatePersonCommand(id, dto), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePersonCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}