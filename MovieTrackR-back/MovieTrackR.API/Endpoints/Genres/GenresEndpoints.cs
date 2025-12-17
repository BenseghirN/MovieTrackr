using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Endpoints.Movies;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.Genres;

public static class GenresEndpoints
{
    public static IEndpointRouteBuilder MapGenresEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/genres")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("Genres")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GenresHandlers.GetAll)
            .WithName("Genres_GetAll")
            .WithSummary("Get all genres")
            .Produces<IReadOnlyList<GenreDto>>(StatusCodes.Status200OK);

        return app;
    }
}