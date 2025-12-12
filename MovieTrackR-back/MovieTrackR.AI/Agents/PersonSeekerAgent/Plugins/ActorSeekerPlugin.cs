using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using MovieTrackR.Application.DTOs;
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
    [Description("Search people based on a query")]
    [return: Description("A IEnumerable of people")]
    public async Task<IReadOnlyList<PersonDetailsDto>> SearchPeopleAsync(CancellationToken cancellationToken = default)
    => await mediator.Send(new GetAllPeopleQuery(), cancellationToken);

    [KernelFunction("get_person_by_id")]
    [Description("Get a details of a person based on is id or tmdbId")]
    [return: Description("A detail list of a person")]
    public async Task<PersonDetailsDto?> GetPersonByIdAsync(Guid? personId, int? tmdbID, CancellationToken cancellationToken = default)
    {
        if (personId is null && tmdbID is null)
            throw new ArgumentException("Provide either personId or tmdbId.");

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
