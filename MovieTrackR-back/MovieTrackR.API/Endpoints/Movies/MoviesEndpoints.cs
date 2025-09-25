using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Configuration;
using MovieTrackR.API.Filters;
using MovieTrackR.API.Validators.Movies;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Movies;

public static class MoviesEndpoints
{
    public static IEndpointRouteBuilder MapMoviesEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/movies")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("Movies")
            .WithOpenApi();

        group.MapGet("/{id:guid}", MoviesHandlers.GetById)
            .WithName("Movies_GetById")
            .WithSummary("Get a movie by id")
            .Produces<MovieDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", MoviesHandlers.Search)
            .WithName("Search_Movies")
            .WithSummary("Search movies (paged)")
            .Produces<IReadOnlyList<MovieDto>>(StatusCodes.Status200OK);

        group.MapPost("/", MoviesHandlers.Create)
            .WithName("Create_Movie")
            .WithSummary("Create a movie")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Accepts<CreateMovieDto>("application/json")
            .AddEndpointFilter<ValidationFilter<CreateMovieDtoValidator>>()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", MoviesHandlers.Update)
            .WithName("Update_Movie")
            .WithSummary("Update a movie")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Accepts<UpdateMovieDto>("application/json")
            .AddEndpointFilter<ValidationFilter<UpdateMovieDtoValidator>>()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", MoviesHandlers.Delete)
            .WithName("Delete_Movie")
            .WithSummary("Delete a movie")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}