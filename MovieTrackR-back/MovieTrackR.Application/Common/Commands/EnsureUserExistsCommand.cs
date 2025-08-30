using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Security;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Common.Commands;

public sealed record EnsureUserExistsCommand(ClaimsPrincipal User) : IRequest<User>;

public sealed class EnsureUserExistsHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<EnsureUserExistsCommand, User>
{
    public async Task<User> Handle(EnsureUserExistsCommand request, CancellationToken cancellationToken)
    {
        User? user = null;
        ClaimsPrincipal claim = request.User;
        string externalId = claim.GetExternalId()
            ?? throw new UnauthorizedAccessException("ExternalId introuvable dans les claims.");

        user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
        if (user is not null) return user;


        string email = claim.GetEmail() ?? string.Empty;
        string display = claim.GetDisplayName() ?? string.Empty;
        string given = claim.GetGivenName() ?? string.Empty;
        string surname = claim.GetSurname() ?? string.Empty;
        string pseudo = !string.IsNullOrWhiteSpace(display)
                         ? display
                         : (!string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : $"user-{Guid.NewGuid():N}".Substring(0, 8));

        user = new User();
        user.Create(externalId, email, pseudo, given, surname);

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }
        catch (DbUpdateException ex)
        {
            // Conflit unique concurrent → relire
            if (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
                return await dbContext.Users.FirstAsync(u => u.ExternalId == externalId, cancellationToken);

            throw;
        }
    }
}