using Asp.Versioning;
using Asp.Versioning.Builder;
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

        return app;
    }
}