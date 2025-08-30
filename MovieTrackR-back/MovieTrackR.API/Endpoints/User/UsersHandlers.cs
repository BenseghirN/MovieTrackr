using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Users.Commands;
using MovieTrackR.Application.Users.Queries;

namespace MovieTrackR.API.Endpoints.User;

/// <summary>Handlers HTTP pour la gestion des utilisateurs (réservé aux administrateurs).</summary>
public static class UsersHandlers
{
    /// <summary>Récupère tous les utilisateurs.</summary>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste des utilisateurs.</returns>
    public static async Task<IResult> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        List<UserDto> list = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Results.Ok(list);
    }

    /// <summary>Récupère un utilisateur par son identifiant.</summary>
    /// <param name="id">ID de l'utilisateur.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Détails de l'utilisateur si trouvé, 404 sinon.</returns>
    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        UserDto? dto = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    /// <summary>Promeut un utilisateur au rôle administrateur.</summary>
    /// <param name="id">ID de l'utilisateur à promouvoir.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'utilisateur mis à jour.</returns>
    public static async Task<IResult> PromoteToAdmin(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        UserDto dto = await mediator.Send(new PromoteToAdminCommand(id), cancellationToken);
        return Results.Ok(dto);
    }

    /// <summary>Rétrograde un administrateur vers le rôle utilisateur.</summary>
    /// <param name="id">ID de l'utilisateur à rétrograder.</param>
    /// <param name="mediator">Médiateur applicatif.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'utilisateur mis à jour.</returns>
    public static async Task<IResult> DemoteToUser(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        UserDto dto = await mediator.Send(new DemoteToUserCommand(id), cancellationToken);
        return Results.Ok(dto);
    }
}