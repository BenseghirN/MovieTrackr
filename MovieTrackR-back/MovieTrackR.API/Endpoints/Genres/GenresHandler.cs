using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Genres.Queries;

namespace MovieTrackR.API.Endpoints.Movies;

/// <summary>Handlers HTTP pour la gestion des genres de film.</summary>
public static class GenresHandlers
{
    /// <summary>Récupère tous les genres de films.</summary>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste des genres de films.</returns>
    public static async Task<IResult> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        IReadOnlyList<GenreDto> genres = await mediator.Send(new GetAllGenresQuery(), cancellationToken);
        return Results.Ok(genres);
    }
}
