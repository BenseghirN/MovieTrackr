using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Common.Services;

public sealed class TmdbCatalogService(
    ITmdbClient tmdbClient,
    IMovieTrackRDbContext dbContext,
    IOptionsSnapshot<TmdbOptions> options,
    ILogger<TmdbCatalogService> logger
) : ITmdbCatalogService
{
    private TmdbOptions TmdbOptions => options.Value;

    public async Task<Guid> ImportMovieAsync(int tmdbId, CancellationToken cancellationToken)
    {
        // 1. déjà importé ?
        Guid existing = await dbContext.Movies.AsNoTracking()
            .Where(m => m.TmdbId == tmdbId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != default)
            return existing;

        // 2. détails TMDb
        string lang = TmdbOptions.DefaultLanguage ?? "fr-FR";
        TmdbMovieDetails details = await tmdbClient.GetMovieDetailsAsync(tmdbId, lang, cancellationToken);

        Movie newMovie = Movie.CreateNew(
            title: details.Title ?? details.OriginalTitle ?? "(Sans titre)",
            tmdbId: tmdbId,
            originalTitle: details.OriginalTitle,
            year: ParseYear(details.ReleaseDate),
            posterUrl: details.PosterPath,
            backdropPath: details.BackdropPath,
            trailerUrl: null,
            duration: details.Runtime,
            overview: details.Overview,
            voteAverage: details.VoteAverage,
            releaseDate: ParseDate(details.ReleaseDate)
        );

        dbContext.Movies.Add(newMovie);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await EnrichMovieAsync(newMovie.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Enrich failed for movie {MovieId} (tmdb {TmdbId})",
                newMovie.Id, tmdbId);
        }
        finally
        {
            // On repart avec un ChangeTracker propre après un import
            foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }
        }

        return newMovie.Id;
    }

    public async Task EnrichMovieAsync(Guid movieId, CancellationToken cancellationToken)
    {
        // Movie TRACKÉ, avec ses collections
        Movie? movie = await dbContext.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.Cast).ThenInclude(m => m.Person)
            .Include(m => m.Crew).ThenInclude(m => m.Person)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);

        if (movie is null)
            throw new KeyNotFoundException("Movie not found.");

        if (movie.TmdbId is null)
            throw new InvalidOperationException("Cannot enrich: movie has no TmdbId.");

        int tmdbId = movie.TmdbId.Value;
        string lang = TmdbOptions.DefaultLanguage ?? "fr-FR";

        // 1 - Détails
        TmdbMovieDetails details = await tmdbClient.GetMovieDetailsAsync(tmdbId, lang, cancellationToken);

        movie.UpdateDetails(
            title: details.Title ?? details.OriginalTitle ?? movie.Title,
            originalTitle: details.OriginalTitle ?? movie.OriginalTitle,
            year: ParseYear(details.ReleaseDate) ?? movie.Year,
            posterUrl: details.PosterPath ?? movie.PosterUrl,
            backdropPath: details.BackdropPath ?? movie.BackdropPath,
            trailerUrl: movie.TrailerUrl,
            duration: details.Runtime ?? movie.Duration,
            overview: details.Overview ?? movie.Overview,
            voteAverage: details.VoteAverage ?? movie.VoteAverage,
            releaseDate: ParseDate(details.ReleaseDate) ?? movie.ReleaseDate
        );
        await dbContext.SaveChangesAsync(cancellationToken);

        // 2 - Genres (ton code existant)
        await EnrichMovieGenres(movie, details, cancellationToken);

        // 3/4 - Cast & Crew
        TmdbMovieCredits credits = await tmdbClient.GetMovieCreditsAsync(tmdbId, lang, cancellationToken);

        List<TmdbCast> castRows = credits.Cast
            .Where(c => c.Id != 0)
            .OrderBy(c => c.Order ?? int.MaxValue)
            .ToList();

        List<TmdbCrew> crewRows = credits.Crew
            .Where(c => c.Id != 0 && !string.IsNullOrWhiteSpace(c.Job))
            .ToList();

        await EnrichMovieCast(movie, castRows, cancellationToken);
        await EnrichMovieCrew(movie, crewRows, cancellationToken);
    }

    #region Helpers

    private static int? ParseYear(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length < 4)
            return null;

        return int.TryParse(s.AsSpan(0, 4), out var y) ? y : (int?)null;
    }

    private static DateTime? ParseDate(string? s)
    {
        return DateTime.TryParse(s, out var d) ? d.Date : null;
    }

    #endregion

    #region Genres

    private async Task EnrichMovieGenres(Movie movie, TmdbMovieDetails details, CancellationToken cancellationToken)
    {
        List<int> tmdbGenreIds = details.Genres.Select(g => g.Id).ToList();

        List<Genre> knownGenres = await dbContext.Genres
            .Where(g => g.TmdbId.HasValue && tmdbGenreIds.Contains(g.TmdbId.Value))
            .ToListAsync(cancellationToken);

        foreach (TmdbGenre g in details.Genres)
        {
            if (!knownGenres.Any(k => k.TmdbId == g.Id))
            {
                knownGenres.Add(Genre.Create(g.Id, g.Name));
            }
        }

        if (knownGenres.Any(k => k.Id == Guid.Empty))
        {
            dbContext.Genres.AddRange(knownGenres.Where(k => k.Id == Guid.Empty));
        }

        movie.MovieGenres.Clear();
        foreach (Genre g in knownGenres)
        {
            movie.AddGenre(g);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Cast

    private async Task EnrichMovieCast(Movie movie, IReadOnlyList<TmdbCast> castList, CancellationToken cancellationToken)
    {
        List<int> castPersonTmdbIds = castList.Select(c => c.Id).Distinct().ToList();
        List<Person> knownCastPeople = await dbContext.People
            .Where(p => p.TmdbId.HasValue && castPersonTmdbIds.Contains(p.TmdbId.Value))
            .ToListAsync(cancellationToken);

        foreach (TmdbCast cast in castList)
            if (!knownCastPeople.Any(p => p.TmdbId == cast.Id))
                knownCastPeople.Add(Person.Create(cast.Name ?? "(Inconnu)", cast.Id, cast.ProfilePath));

        if (knownCastPeople.Any(p => p.Id == Guid.Empty))
            dbContext.People.AddRange(knownCastPeople.Where(p => p.Id == Guid.Empty));
        await dbContext.SaveChangesAsync(cancellationToken);

        movie.Cast.Clear();
        foreach (TmdbCast cast in castList)
        {
            Person person = knownCastPeople.First(p => p.TmdbId == cast.Id);
            movie.AddCast(person, cast.Character, cast.Order);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Crew

    private async Task EnrichMovieCrew(Movie movie, IReadOnlyList<TmdbCrew> crewList, CancellationToken cancellationToken)
    {
        List<int> crewPersonTmdbIds = crewList.Select(c => c.Id).Distinct().ToList();
        List<Person> knownCrewPeople = await dbContext.People
            .Where(p => p.TmdbId.HasValue && crewPersonTmdbIds.Contains(p.TmdbId.Value))
            .ToListAsync(cancellationToken);

        foreach (TmdbCrew crew in crewList)
            if (!knownCrewPeople.Any(p => p.TmdbId == crew.Id))
                knownCrewPeople.Add(Person.Create(crew.Name ?? "(Inconnu)", crew.Id, crew.ProfilePath));

        if (knownCrewPeople.Any(p => p.Id == Guid.Empty))
            dbContext.People.AddRange(knownCrewPeople.Where(p => p.Id == Guid.Empty));
        await dbContext.SaveChangesAsync(cancellationToken);

        movie.Crew.Clear();
        foreach (TmdbCrew crew in crewList)
        {
            Person person = knownCrewPeople.First(p => p.TmdbId == crew.Id);
            movie.AddCrew(person, crew.Job!, crew.Department);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
