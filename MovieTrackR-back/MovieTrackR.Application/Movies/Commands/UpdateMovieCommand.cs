using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Commands;

public sealed record UpdateMovieCommand(Guid Id, UpdateMovieDto dto) : IRequest;

public sealed class UpdateMovieHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<UpdateMovieCommand>
{
    public async Task Handle(UpdateMovieCommand updateCommand, CancellationToken cancellationToken)
    {
        Movie movie = await dbContext.Movies
            .Include(x => x.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(x => x.Id == updateCommand.Id, cancellationToken)
            ?? throw new NotFoundException("Movie not found.");

        bool exists = await dbContext.Movies.AnyAsync(
            x => x.Id != updateCommand.Id && x.Title == updateCommand.dto.Title && x.Year == updateCommand.dto.Year, cancellationToken);
        if (exists) throw new ConflictException("Another movie with same title & year exists.");

        movie.UpdateDetails(
            title: updateCommand.dto.Title.Trim(),
            originalTitle: updateCommand.dto.OriginalTitle,
            year: updateCommand.dto.Year,
            posterUrl: updateCommand.dto.PosterUrl,
            backdropPath: updateCommand.dto.BackdropPath,
            trailerUrl: updateCommand.dto.TrailerUrl,
            duration: updateCommand.dto.Duration,
            overview: updateCommand.dto.Overview,
            releaseDate: updateCommand.dto.ReleaseDate,
            voteAverage: updateCommand.dto.VoteAverage
        );

        HashSet<Guid> currentGenres = movie.MovieGenres.Select(mg => mg.GenreId).ToHashSet();
        HashSet<Guid> newGenres = (updateCommand.dto.GenreIds ?? Array.Empty<Guid>()).ToHashSet();

        if (newGenres.Count > 0)
        {
            List<Genre> genres = await dbContext.Genres.Where(g => newGenres.Contains(g.Id)).ToListAsync(cancellationToken);
            foreach (Genre g in genres.Where(g => !currentGenres.Contains(g.Id)))
                movie.AddGenre(g);
        }

        foreach (Guid toRemove in currentGenres.Except(newGenres))
            movie.RemoveGenre(toRemove);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}