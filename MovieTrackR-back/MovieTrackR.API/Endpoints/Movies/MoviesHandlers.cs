using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Movies.Commands;
using MovieTrackR.Application.Movies.Queries;

namespace MovieTrackR.API.Endpoints.Movies;

/// <summary>Handlers HTTP pour la gestion des films (réservé aux administrateurs).</summary>
public static class MoviesHandlers
{
    /// <summary>Récupère un film par son identifiant.</summary>
    /// <param name="id">ID du film.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le film si trouvé, 404 sinon.</returns>
    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        MovieDto? dto = await mediator.Send(new GetMovieByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    /// <summary>Recherche paginée de films par critères.</summary>
    /// <param name="query">Paramètres de recherche (query string).</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="response">Réponse HTTP (ajoute l'entête X-Total-Count).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste paginée de films.</returns>
    public static async Task<IResult> Search([AsParameters] MovieSearchRequest query, IMediator mediator, HttpResponse response, CancellationToken cancellationToken)
    {
        HybridPagedResult<SearchMovieResultDto> result =
        await mediator.Send(new SearchMoviesHybridQuery(query.ToCriteria()), cancellationToken);

        if (result.Meta.TotalResults is int tr)
            response.Headers["X-Total-Tmdb"] = tr.ToString();

        response.Headers["X-Total-Local"] = result.Meta.TotalLocal.ToString();
        response.Headers["Access-Control-Expose-Headers"] = "X-Total-Tmdb, X-Total-Local";
        return Results.Ok(result);
    }


    /// <summary>Crée un film (réservé aux administrateurs).</summary>
    /// <param name="dto">Données de création.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>201 Created + identifiant du film.</returns>
    public static async Task<IResult> Create(CreateMovieDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        Guid id = await mediator.Send(new CreateMovieCommand(dto), cancellationToken);
        return Results.Created($"/movies/{id}", new { id });
    }


    /// <summary>Met à jour un film (réservé aux administrateurs).</summary>
    /// <param name="id">ID du film.</param>
    /// <param name="dto">Données de mise à jour.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>204 No Content.</returns>
    public static async Task<IResult> Update(Guid id, UpdateMovieDto dto, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateMovieCommand(id, dto), cancellationToken);
        return Results.NoContent();
    }

    /// <summary>Supprime un film (réservé aux administrateurs).</summary>
    /// <param name="id">ID du film.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>204 No Content.</returns>
    public static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteMovieCommand(id), cancellationToken);
        return Results.NoContent();
    }
}