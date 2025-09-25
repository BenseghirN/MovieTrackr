using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Common.Commands;

public sealed record EnsureUserExistsCommand(string externalId, string email, string display, string given, string surname, string pseudo) : IRequest<Guid>;

public sealed class EnsureUserExistsHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<EnsureUserExistsCommand, Guid>
{
    public async Task<Guid> Handle(EnsureUserExistsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.externalId))
            throw new ArgumentException("ExternalId est requis.", nameof(request.externalId));
        Guid existingUser = await dbContext.Users
            .Where(u => u.ExternalId == request.externalId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser != Guid.Empty) return existingUser;

        User user = new User();
        user.Create(request.externalId, request.email, request.pseudo, request.given, request.surname);

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return user.Id;
        }
        catch (DbUpdateException ex)
        {
            // Conflit de création (concurrence) : un autre processus a créé l'utilisateur entre temps.
            if (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
                return await dbContext.Users
                            .Where(u => u.ExternalId == request.externalId)
                            .Select(u => u.Id)
                            .FirstOrDefaultAsync(cancellationToken);
            throw;
        }
    }
}