using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB.Interfaces;
using Npgsql;

namespace MovieTrackR.Application.Movies.Commands;

public sealed record EnsureLocalMovieCommand(Guid? MovieId, int? TmdbId) : IRequest<Guid>;

public sealed class EnsureLocalMovieHandler(IMovieTrackRDbContext dbContext, ITmdbCatalogService tmdbCatalogService) : IRequestHandler<EnsureLocalMovieCommand, Guid>
{
    public async Task<Guid> Handle(EnsureLocalMovieCommand command, CancellationToken cancellationToken)
    {
        if (command.MovieId is Guid localId)
        {
            bool exists = await dbContext.Movies.AnyAsync(m => m.Id == localId, cancellationToken);
            if (!exists)
                throw new NotFoundException("Movie", localId);
            return localId;
        }

        if (command.TmdbId is null)
            throw new InvalidOperationException("Either MovieId or TmdbId must be provided.");

        // Movie exists ?
        Guid existing = await dbContext.Movies
            .AsNoTracking()
            .Where(m => m.TmdbId == command.TmdbId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != Guid.Empty) return existing;

        // If not, import it
        try
        {
            Guid movieId = await tmdbCatalogService.ImportMovieAsync(command.TmdbId.Value, cancellationToken);
            return movieId;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Conflit : un autre thread l’a inséré entre-temps → on relit
            return await dbContext.Movies
                .Where(m => m.TmdbId == command.TmdbId)
                .Select(m => m.Id)
                .FirstAsync(cancellationToken);
        }
    }
}