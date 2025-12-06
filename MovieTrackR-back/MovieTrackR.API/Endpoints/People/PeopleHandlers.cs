using MediatR;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.People.Commands;
using MovieTrackR.Application.People.Queries;

namespace MovieTrackR.API.Endpoints.People;

/// <summary>Handlers HTTP pour la gestion des personnes (acteurs, réalisateurs, etc.).</summary>
public static class PeopleHandlers
{
    /// <summary>Recherche des personnes par nom (résultats paginés avec fusion DB locale + TMDB).</summary>
    /// <param name="query">Critères de recherche (nom, page, taille de page).</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="response">Réponse HTTP (pour ajouter les headers de pagination).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste paginée de personnes correspondant aux critères.</returns>
    public static async Task<IResult> Search([AsParameters] PeopleSearchRequest query, IMediator mediator, HttpResponse response, CancellationToken cancellationToken)
    {
        HybridPagedResult<SearchPersonResultDto> result =
        await mediator.Send(new SearchPeopleQuery(query.ToCriteria()), cancellationToken);

        response.Headers["X-Total-Local"] = result.Meta.TotalLocal.ToString();
        response.Headers["X-Total-Tmdb"] = result.Meta.TotalTmdb.ToString();
        response.Headers["X-Total"] = result.Meta.TotalResults.ToString();
        return TypedResults.Ok(result);
    }

    /// <summary>Récupère une personne par son identifiant local.</summary>
    /// <param name="id">ID de la personne (GUID local).</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Les détails de la personne si trouvée, 404 sinon.</returns>
    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        PersonDetailsDto? dto = await mediator.Send(new GetPersonByIdQuery(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    /// <summary>Récupère une personne TMDB (l'importe si nécessaire).</summary>
    /// <param name="tmdbId">ID TMDB de la personne.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Les détails de la personne si trouvée, 404 sinon.</returns>
    /// <remarks>
    /// Cet endpoint est utilisé quand l'utilisateur clique sur une personne issue d'une recherche TMDB
    /// qui n'existe pas encore en base locale. La personne sera automatiquement importée avec tous ses détails.
    /// </remarks>
    public static async Task<IResult> GetByTmdbId(int tmdbId, IMediator mediator, CancellationToken cancellationToken)
    {
        try
        {
            Guid localId = await mediator.Send(
                new EnsureLocalPersonCommand(PersonId: null, TmdbId: tmdbId),
                cancellationToken);

            PersonDetailsDto? dto = await mediator.Send(
                new GetPersonByIdQuery(localId),
                cancellationToken);

            return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound(new { error = $"Person TMDB {tmdbId} introuvable" });
        }
    }

    /// <summary>Récupère la filmographie d'une personne (cast + crew important).</summary>
    /// <param name="id">ID de la personne (GUID local).</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des crédits films (acteur + équipe technique), 404 si personne non trouvée.</returns>
    /// <remarks>
    /// Fusionne automatiquement les crédits de la base locale avec ceux de TMDB si moins de 10 films.
    /// Retourne jusqu'à 20 films aléatoires pour une section "Also played in".
    /// </remarks>
    public static async Task<IResult> GetPersonMovieCredits(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        List<PersonMovieCreditDto>? dto = await mediator.Send(new GetPersonMovieCreditsQuery(id), cancellationToken);
        return TypedResults.Ok(dto);
    }
}