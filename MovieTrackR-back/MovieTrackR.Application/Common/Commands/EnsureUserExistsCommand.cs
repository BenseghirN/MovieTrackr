using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Common.Commands;

public sealed record EnsureUserExistsCommand(CurrentUserDto currentUser) : IRequest<Guid>;

public sealed class EnsureUserExistsHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<EnsureUserExistsCommand, Guid>
{
    public async Task<Guid> Handle(EnsureUserExistsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.currentUser.ExternalId))
            throw new ArgumentException("ExternalId est requis.", nameof(request.currentUser.ExternalId));
        Guid existingUser = await dbContext.Users
            .Where(u => u.ExternalId == request.currentUser.ExternalId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser != Guid.Empty) return existingUser;

        if (string.IsNullOrWhiteSpace(request.currentUser.Email))
            throw new ArgumentException("Email est requis.", nameof(request.currentUser.Email));

        User user = User.Create(
            request.currentUser.ExternalId,
            request.currentUser.Email,
            request.currentUser.DisplayName ?? string.Empty,
            request.currentUser.GivenName ?? string.Empty,
            request.currentUser.Surname ?? string.Empty
        );

        dbContext.Users.Add(user);


        try
        {
            // save User
            await dbContext.SaveChangesAsync(cancellationToken);
            createDefaultSystemLists(user);
            // save lists who neede user to be created
            await dbContext.SaveChangesAsync(cancellationToken);
            return user.Id;
        }
        catch (DbUpdateException ex)
        {
            // Conflit de création (concurrence) : un autre processus a créé l'utilisateur entre temps.
            if (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
                return await dbContext.Users
                            .Where(u => u.ExternalId == request.currentUser.ExternalId)
                            .Select(u => u.Id)
                            .FirstOrDefaultAsync(cancellationToken);
            throw;
        }
    }

    private void createDefaultSystemLists(User user)
    {
        UserList watchListDefault = UserList.CreateWatchlist(user.Id);
        UserList favoritesDefault = UserList.CreateFavorites(user.Id);
        dbContext.UserLists.AddRange(watchListDefault, favoritesDefault);
    }
}