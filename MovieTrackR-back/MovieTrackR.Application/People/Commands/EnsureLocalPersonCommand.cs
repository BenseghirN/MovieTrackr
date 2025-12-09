using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB.Interfaces;
using Npgsql;

namespace MovieTrackR.Application.People.Commands;

public sealed record EnsureLocalPersonCommand(Guid? PersonId, int? TmdbId) : IRequest<Guid>;

public sealed class EnsureLocalPersonHandler(IMovieTrackRDbContext dbContext, ITmdbCatalogService tmdbCatalogService) : IRequestHandler<EnsureLocalPersonCommand, Guid>
{
    public async Task<Guid> Handle(EnsureLocalPersonCommand command, CancellationToken cancellationToken)
    {
        if (command.PersonId is Guid localId)
        {
            bool exists = await dbContext.People.AnyAsync(m => m.Id == localId, cancellationToken);
            if (!exists)
                throw new NotFoundException("Person", localId);
            return localId;
        }

        if (command.TmdbId is null)
            throw new InvalidOperationException("Either PersonId or TmdbId must be provided.");

        // Person exists ?
        Guid existing = await dbContext.People
            .AsNoTracking()
            .Where(m => m.TmdbId == command.TmdbId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != Guid.Empty) return existing;

        // If not, import it
        try
        {
            Guid personId = await tmdbCatalogService.EnsurePersonExistsAsync(command.TmdbId.Value, cancellationToken);
            return personId;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Conflit : un autre thread l’a inséré entre-temps → on relit
            return await dbContext.People
                .Where(m => m.TmdbId == command.TmdbId)
                .Select(m => m.Id)
                .FirstAsync(cancellationToken);
        }
    }
}