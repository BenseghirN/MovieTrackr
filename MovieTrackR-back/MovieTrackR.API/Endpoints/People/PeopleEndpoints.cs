using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Configuration;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.People;

public static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/people")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("People")
            .WithOpenApi();

        group.MapGet("/", PeopleHandlers.GetAll)
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .WithName("People_GetAll")
            .WithSummary("Get list of all people")
            .Produces<IReadOnlyList<PersonDetailsDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/search", PeopleHandlers.Search)
            .AllowAnonymous()
            .WithName("Search_People")
            .WithSummary("Search people (paged)")
            .Produces<IReadOnlyList<SearchPersonResultDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", PeopleHandlers.GetById)
            .AllowAnonymous()
            .WithName("Person_GetById")
            .WithSummary("Get a person by id")
            .Produces<PersonDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/tmdb/{tmdbId:int}", PeopleHandlers.GetByTmdbId)
            .AllowAnonymous()
            .WithName("Person_GetByTmdbId")
            .WithSummary("Get person details from TMDB by TMDB id")
            .Produces<PersonDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/credits", PeopleHandlers.GetPersonMovieCredits)
            .AllowAnonymous()
            .WithName("People_GetMovieCredits")
            .WithSummary("Get credits for a person")
            .Produces<IReadOnlyList<PersonMovieCreditDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", PeopleHandlers.Create)
            .WithName("Create_Person")
            .WithSummary("Create a person")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Accepts<CreatePersonDto>("application/json")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", PeopleHandlers.Update)
            .WithName("Update_Person")
            .WithSummary("Update a person")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Accepts<UpdatePersonDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", PeopleHandlers.Delete)
            .WithName("Delete_Person")
            .WithSummary("Delete a person")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}