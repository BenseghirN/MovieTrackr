using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.People;
using MovieTrackR.Application.People.Commands;
using MovieTrackR.Application.People.Queries;

namespace MovieTrackR.AI.Agents.ActorSeekerAgent.Plugins;

public sealed class ActorSeekerPlugin(IMediator mediator)
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

        var criteria = new PeopleSearchCriteria
        {
            Query = query.Trim(),
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 10) // important: keep it small for tokens/quota
        };

        HybridPagedResult<SearchPersonResultDto> result = await mediator.Send(new SearchPeopleQuery(criteria), cancellationToken);
        return result.Items;
    }

    [KernelFunction("get_person_by_id")]
    [Description("Get a details of a person based on is id or tmdbId")]
    [return: Description("A detail list of a person")]
    public async Task<PersonDetailsDto?> GetPersonByIdAsync(Guid? personId, int? tmdbID, CancellationToken cancellationToken = default)
    {
        if (personId is null && tmdbID is null)
            throw new ArgumentException("Provide either personId or tmdbId.");
        Console.WriteLine("🔥 ActorSeekerPlugin.GetPersonByIdAsync called");

        Guid localId;

        if (personId is not null)
        {
            localId = personId.Value;
        }
        else
        {
            localId = await mediator.Send(
                new EnsureLocalPersonCommand(PersonId: null, TmdbId: tmdbID),
                cancellationToken);
        }

        return await mediator.Send(new GetPersonByIdQuery(localId), cancellationToken);
    }

    // [KernelFunction("get_user")]
    // [Description("Get a user by first and last name")]
    // [return: Description("A user")]
    // public Task<User?> GetUserAsync(string firstName, string lastName)
    // {
    //     return Task.FromResult(_mediator.GetUser(firstName, lastName));
    // }

    // [KernelFunction("create_user")]
    // [Description("Create a new user")]
    // public Task CreateUserAsync(string firstName, string lastName, int age)
    // {
    //     _mediator.CreateUser(firstName, lastName, age);
    //     return Task.CompletedTask;
    // }

    // [KernelFunction("delete_user")]
    // [Description("Delete a user by first and last name")]
    // public Task DeleteUserAsync(string firstName, string lastName)
    // {
    //     _mediator.deleteUser(firstName, lastName);
    //     return Task.CompletedTask;
    // }
}
