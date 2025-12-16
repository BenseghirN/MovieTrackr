using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.People;
using MovieTrackR.Application.People.Commands;
using MovieTrackR.Application.People.Queries;

namespace MovieTrackR.AI.Agents.ActorSeekerAgent.Plugins;

public sealed class PersonSeekerPlugin(IMediator mediator)
{


    [KernelFunction("get_all_people")]
    [Description("Get a list of all people")]
    [return: Description("A IEnumerable of people")]
    public async Task<IReadOnlyList<PersonDetailsDto>> GetAllPeopleAsync(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetAllPeopleQuery(), cancellationToken);

    [KernelFunction("search_people")]
    [Description("Search people in MovieTrackR by name (local database). Returns a short list of matches.")]
    public async Task<IReadOnlyList<SearchPersonResultDto>> SearchPeopleAsync(
        [Description("Name or partial name to search for (e.g. 'Damien Chazelle')")] string query,
        [Description("Page number (1-based). Default is 1.")] int page = 1,
        [Description("How many results to return (1..10). Default is 5.")] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<SearchPersonResultDto>();

        PeopleSearchCriteria criteria = new PeopleSearchCriteria
        {
            Query = query.Trim(),
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 10)
        };

        HybridPagedResult<SearchPersonResultDto> result = await mediator.Send(new SearchPeopleQuery(criteria), cancellationToken);
        return result.Items;
    }

    [KernelFunction("get_person_by_id")]
    [Description("Get a details of a person based on is id or tmdbId")]
    [return: Description("A detail list of a person")]
    public async Task<PersonDetailsDto?> GetPersonByIdAsync(
        [Description("Local database id of the person (e.g. 'a2b8dcdc-cac4-11f0-91b8-cb53db0d0acc')")] Guid? localPersonId,
        [Description("Id of the person on tmdb website (e.g. '2310748')")] int? tmdbPersonId,
        CancellationToken cancellationToken = default)
    {
        if (localPersonId is null && tmdbPersonId is null)
            throw new ArgumentException("Provide either localPersonId or tmdbPersonId.");

        Guid localId;

        if (localPersonId is not null)
        {
            localId = localPersonId.Value;
        }
        else
        {
            localId = await mediator.Send(
                new EnsureLocalPersonCommand(PersonId: null, TmdbId: tmdbPersonId),
                cancellationToken);
        }

        return await mediator.Send(new GetPersonByIdQuery(localId), cancellationToken);
    }
}
