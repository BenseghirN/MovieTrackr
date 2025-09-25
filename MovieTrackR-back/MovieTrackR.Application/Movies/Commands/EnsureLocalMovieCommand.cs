// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using MovieTrackR.Application.Common.Exceptions;
// using MovieTrackR.Application.Interfaces;
// using MovieTrackR.Domain.Entities;
// using Npgsql;

// namespace MovieTrackR.Application.Movies.Commands;

// public sealed record EnsureLocalMovieCommand(Guid? MovieId, int? TmdbId) : IRequest<Guid>;

// public sealed class EnsureLocalMovieHandler(IMovieTrackRDbContext dbContext,
//     ITmdbClient tmdb)
//     : IRequestHandler<EnsureLocalMovieCommand, Guid>
// {
//     public async Task<Guid> Handle(EnsureLocalMovieCommand command, CancellationToken cancellationToken)
//     {
//         if (command.MovieId is Guid id) return id;
//         if (command.TmdbId is null) throw new InvalidOperationException("Provide movieId or tmdbId.");

//         // 1) déjà en base par TmdbId ?
//         Guid existing = await dbContext.Movies
//             .Where(m => m.TmdbId == command.TmdbId)
//             .Select(m => m.Id)
//             .FirstOrDefaultAsync(cancellationToken);
//         if (existing != Guid.Empty) return existing;

//         // 2) fetch détails TMDb (v1 sans credits)
//         var d = await tmdb.GetMovieDetailsAsync(command.TmdbId.Value, includeCredits: false, cancellationToken)
//                  ?? throw new NotFoundException("TMDb movie", command.TmdbId);

//         int? year = null; DateTime? release = null;
//         if (!string.IsNullOrWhiteSpace(d.ReleaseDate))
//         {
//             if (int.TryParse(d.ReleaseDate[..Math.Min(4, d.ReleaseDate.Length)], out var y)) year = y;
//             if (DateTime.TryParse(d.ReleaseDate, out var dt)) release = dt;
//         }

//         Movie movie = Movie.CreateNew(
//             title: d.Title,
//             tmdbId: d.Id,
//             originalTitle: d.OriginalTitle,
//             year: year,
//             posterUrl: tmdb.BuildPosterUrl(d.PosterPath),
//             trailerUrl: null,
//             duration: d.Runtime,
//             overview: d.Overview,
//             releaseDate: release
//         );

//         // Genres “light”
//         if (d.Genres?.Count > 0)
//         {
//             var names = d.Genres.Select(g => g.Name).ToList();
//             var genres = await dbContext.Genres.Where(g => names.Contains(g.Name)).ToListAsync(cancellationToken);

//             var missing = names.Except(genres.Select(g => g.Name), StringComparer.OrdinalIgnoreCase).ToList();
//             foreach (var name in missing)
//             {
//                 var newGenre = Genre.Create(tmdbId: null, name: name);
//                 dbContext.Genres.Add(newGenre);
//                 genres.Add(newGenre);
//             }
//             foreach (var g in genres) movie.AddGenre(g);
//         }

//         dbContext.Movies.Add(movie);
//         try
//         {
//             await dbContext.SaveChangesAsync(cancellationToken);
//             return movie.Id;
//         }
//         catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
//         {
//             // course condition: quelqu’un vient de l’insérer → on relit
//             return await dbContext.Movies.Where(m => m.TmdbId == command.TmdbId)
//                                   .Select(m => m.Id)
//                                   .FirstAsync(cancellationToken);
//         }
//     }
// }