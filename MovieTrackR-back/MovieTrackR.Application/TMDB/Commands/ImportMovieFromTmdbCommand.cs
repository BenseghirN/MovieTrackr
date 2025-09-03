// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using MovieTrackR.Application.Interfaces;
// using MovieTrackR.Domain.Entities;

// namespace MovieTrackR.Application.TMDB.Commands;

// public sealed record ImportMovieFromTmdbCommand(int TmdbId, bool Merge = true) : IRequest<Guid>;

// public sealed class ImportMovieFromTmdbHandler(IMovieTrackRDbContext dbContext, ITmdbClient tmdb)
//     : IRequestHandler<ImportMovieFromTmdbCommand, Guid>
// {
//     public async Task<Guid> Handle(ImportMovieFromTmdbCommand r, CancellationToken ct)
//     {
//         Movie? existing = await dbContext.Movies.FirstOrDefaultAsync(m => m.TmdbId == r.TmdbId, ct);
//         if (existing is not null)
//             return existing.Id; // ou merge si r.Merge && données TMDb plus fraîches

//         var dto = await tmdb.GetMovieAsync(r.TmdbId, ct); // contient titres, overview, année, genres, people...

//         var movie = Movie.CreateNew(
//             title: dto.Title,
//             tmdbId: r.TmdbId,
//             originalTitle: dto.OriginalTitle,
//             year: dto.Year,
//             posterUrl: dto.PosterUrl,
//             trailerUrl: dto.TrailerUrl,
//             duration: dto.DurationMin,
//             overview: dto.Overview,
//             releaseDate: dto.ReleaseDate);

//         List<Genre> genres = await dbContext.Genres.Where(g => dto.GenreNames.Contains(g.Name)).ToListAsync(ct);
//         var missing = dto.GenreNames.Except(genres.Select(g => g.Name)).ToList();
//         foreach (var name in missing)
//         {
//             Genre newGenre = Genre.Create(TmdbId: TmdbId, name: name);
//             dbContext.Genres.Add(newGenre);
//             genres.Add(newGenre);
//         }
//         foreach (var g in genres) movie.AddGenre(g);

//         dbContext.Movies.Add(movie);
//         await dbContext.SaveChangesAsync(ct);
//         return movie.Id;
//     }
// }
