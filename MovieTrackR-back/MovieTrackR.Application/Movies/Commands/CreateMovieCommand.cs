using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Commands;

public sealed record CreateMovieCommand(CreateMovieDto Dto) : IRequest<Guid>;

public sealed class CreateMovieHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<CreateMovieCommand, Guid>
{
    public async Task<Guid> Handle(CreateMovieCommand createCommand, CancellationToken cancellationToken)
    {
        // unicité (Title+Year) — Year nullable : OK (DB considère NULL distinct)
        bool exists = await dbContext.Movies.AnyAsync(
            m => m.Title == createCommand.Dto.Title && m.Year == createCommand.Dto.Year, cancellationToken);
        if (exists) throw new ConflictException("Movie already exists (title+year).");

        Movie movie = Movie.CreateNew(
            title: createCommand.Dto.Title.Trim(),
            tmdbId: createCommand.Dto.TmdbId,
            originalTitle: createCommand.Dto.OriginalTitle,
            year: createCommand.Dto.Year,
            posterUrl: createCommand.Dto.PosterUrl,
            backdropPath: createCommand.Dto.BackdropPath,
            trailerUrl: createCommand.Dto.TrailerUrl,
            duration: createCommand.Dto.Duration,
            overview: createCommand.Dto.Overview,
            releaseDate: createCommand.Dto.ReleaseDate,
            voteAverage: createCommand.Dto.VoteAverage
        );

        if (createCommand.Dto.GenreIds?.Count > 0)
        {
            List<Genre> genres = await dbContext.Genres.Where(g => createCommand.Dto.GenreIds.Contains(g.Id)).ToListAsync(cancellationToken);
            foreach (Genre g in genres) movie.AddGenre(g);
        }

        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
        return movie.Id;
    }
}