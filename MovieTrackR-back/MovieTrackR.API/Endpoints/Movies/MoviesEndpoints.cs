using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Configuration;
using MovieTrackR.API.Filters;
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
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/{id:guid}", MoviesHandlers.GetById)
            .AllowAnonymous()
            .WithName("Movies_GetById")
            .WithSummary("Get a movie by id")
            .Produces<MovieDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", MoviesHandlers.GetAllMovies)
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .WithName("Movies_GetAll")
            .WithSummary("Get list of all movies")
            .Produces<IReadOnlyList<MovieAdminDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/tmdb/{tmdbId:int}", MoviesHandlers.GetByTmdbId)
            .AllowAnonymous()
            .WithName("Movies_GetByTmdbId")
            .WithSummary("Get movie details from TMDB by TMDB id")
            .Produces<MovieDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/search", MoviesHandlers.Search)
            .WithName("Search_Movies")
            .WithSummary("Search movies (paged)")
            .AllowAnonymous()
            .Produces<HybridPagedResult<SearchMovieResultDto>>(StatusCodes.Status200OK);

        group.MapGet("/popular", MoviesHandlers.GetPopular)
            .WithName("Get_Popular")
            .WithSummary("Get popular/newest movies from TMDB (paged)")
            .AllowAnonymous()
            .Produces<HybridPagedResult<SearchMovieResultDto>>(StatusCodes.Status200OK);

        group.MapGet("/trending", MoviesHandlers.GetTrending)
            .WithName("Get_Trending")
            .WithSummary("Top 20 trending local movies")
            .WithDescription("Basé sur l'activité locale : reviews, commentaires, likes.")
            .AllowAnonymous()
            .Produces<IReadOnlyList<SearchMovieResultDto>>(StatusCodes.Status200OK);

        group.MapPost("/", MoviesHandlers.Create)
            .WithName("Create_Movie")
            .WithSummary("Create a movie")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Accepts<CreateMovieDto>("application/json")
            .AddEndpointFilter<ValidationFilter<CreateMovieDto>>()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", MoviesHandlers.Update)
            .WithName("Update_Movie")
            .WithSummary("Update a movie")
            .RequireAuthorization(AuthorizationConfiguration.AdminPolicy)
            .Accepts<UpdateMovieDto>("application/json")
            .AddEndpointFilter<ValidationFilter<UpdateMovieDto>>()
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

        group.MapGet("/{tmdbId:int}/streaming", MoviesHandlers.GetMovieStreamingOffers)
            .AllowAnonymous()
            .WithName("Movies_GetStreamingProviders")
            .WithSummary("Get streaming providers for a TMDb movie")
            .Produces<StreamingOfferDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}