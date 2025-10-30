using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Common.Services;

public sealed class TmdbCatalogService(ITmdbClient tmdbClient, IMovieTrackRDbContext dbContext, IOptionsSnapshot<TmdbOptions> options, ILogger<TmdbCatalogService> logger) : ITmdbCatalogService
{
    private TmdbOptions tmdbOptions => options.Value;
    public async Task EnrichMovieAsync(Guid movieId, CancellationToken cancellationToken)
    {
        Movie? movie = await dbContext.Movies
            .Include(m => m.MovieGenres)
            .Include(m => m.Cast)
            .Include(m => m.Crew)
            .FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);

        if (movie is null) throw new KeyNotFoundException("Movie not found.");
        if (movie.TmdbId is null) throw new InvalidOperationException("Cannot enrich: movie has no TmdbId.");

        int tmdbId = movie.TmdbId.Value;
        string lang = tmdbOptions.DefaultLanguage ?? "fr-FR";

        TmdbMovieDetails details = await tmdbClient.GetMovieDetailsAsync(tmdbId, lang, cancellationToken);
        TmdbMovieCredits credits = await tmdbClient.GetMovieCreditsAsync(tmdbId, cancellationToken);

        // 1 - Détails
        movie.UpdateDetails(
            title: details.Title ?? details.OriginalTitle ?? movie.Title,
            originalTitle: details.OriginalTitle ?? movie.OriginalTitle,
            year: ParseYear(details.ReleaseDate) ?? movie.Year,
            posterUrl: details.PosterPath ?? movie.PosterUrl,
            trailerUrl: movie.TrailerUrl,
            duration: details.Runtime ?? movie.Duration,
            overview: details.Overview ?? movie.Overview,
            releaseDate: ParseDate(details.ReleaseDate) ?? movie.ReleaseDate
        );

        // 2 - Genres
        List<int> tmdbGenreIds = details.Genres.Select(g => g.Id).ToList();
        List<Genre> knownGenres = await dbContext.Genres
            .Where(g => g.TmdbId.HasValue && tmdbGenreIds.Contains(g.TmdbId.Value))
            .ToListAsync(cancellationToken);

        foreach (TmdbGenre g in details.Genres)
            if (!knownGenres.Any(k => k.TmdbId == g.Id))
                knownGenres.Add(Genre.Create(g.Id, g.Name));

        if (knownGenres.Any(k => k.Id == Guid.Empty))
            dbContext.Genres.AddRange(knownGenres.Where(k => k.Id == Guid.Empty));
        await dbContext.SaveChangesAsync(cancellationToken);

        movie.MovieGenres.Clear();
        foreach (Genre g in knownGenres)
            movie.AddGenre(g);

        // 3 - Cast
        List<TmdbCast> castRows = credits.Cast
            .Where(c => c.Id != 0)
            .OrderBy(c => c.Order ?? int.MaxValue)
            .Take(50)
            .ToList();

        List<int> castPersonTmdbIds = castRows.Select(c => c.Id).Distinct().ToList();
        List<Person> knownCastPeople = await dbContext.People
            .Where(p => p.TmdbId.HasValue && castPersonTmdbIds.Contains(p.TmdbId.Value))
            .ToListAsync(cancellationToken);

        foreach (TmdbCast cast in castRows)
            if (!knownCastPeople.Any(p => p.TmdbId == cast.Id))
                knownCastPeople.Add(Person.Create(cast.Name ?? "(Inconnu)", cast.Id));

        if (knownCastPeople.Any(p => p.Id == Guid.Empty))
            dbContext.People.AddRange(knownCastPeople.Where(p => p.Id == Guid.Empty));
        await dbContext.SaveChangesAsync(cancellationToken);

        movie.Cast.Clear();
        foreach (TmdbCast cast in castRows)
        {
            Person person = knownCastPeople.First(p => p.TmdbId == cast.Id);
            movie.AddCast(person, cast.Character, cast.Order);
        }

        // 4 - Crew
        List<TmdbCrew> crewRows = credits.Crew
            .Where(c => c.Id != 0 && !string.IsNullOrWhiteSpace(c.Job))
            .Take(100)
            .ToList();

        List<int> crewPersonTmdbIds = crewRows.Select(c => c.Id).Distinct().ToList();
        List<Person> knownCrewPeople = await dbContext.People
            .Where(p => p.TmdbId.HasValue && crewPersonTmdbIds.Contains(p.TmdbId.Value))
            .ToListAsync(cancellationToken);

        foreach (TmdbCrew crew in crewRows)
            if (!knownCrewPeople.Any(p => p.TmdbId == crew.Id))
                knownCrewPeople.Add(Person.Create(crew.Name ?? "(Inconnu)", crew.Id));

        if (knownCrewPeople.Any(p => p.Id == Guid.Empty))
            dbContext.People.AddRange(knownCrewPeople.Where(p => p.Id == Guid.Empty));
        await dbContext.SaveChangesAsync(cancellationToken);

        movie.Crew.Clear();
        foreach (TmdbCrew crew in crewRows)
        {
            Person person = knownCrewPeople.First(p => p.TmdbId == crew.Id);
            movie.AddCrew(person, crew.Job!, crew.Department);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<Guid> ImportMovieAsync(int tmdbId, CancellationToken cancellationToken)
    {
        // already imported?
        Guid existing = await dbContext.Movies.AsNoTracking()
            .Where(m => m.TmdbId == tmdbId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != default) return existing;

        // détails TMDb (lang par défaut depuis options, fallback fr-FR)
        string lang = tmdbOptions.DefaultLanguage ?? "fr-FR";
        TmdbMovieDetails details = await tmdbClient.GetMovieDetailsAsync(tmdbId, lang, cancellationToken);

        // seed minimal
        Movie newMovie = Movie.CreateNew(
            title: details.Title ?? details.OriginalTitle ?? "(Sans titre)",
            tmdbId: tmdbId,
            originalTitle: details.OriginalTitle,
            year: ParseYear(details.ReleaseDate),
            posterUrl: details.PosterPath,
            trailerUrl: null,
            duration: details.Runtime,
            overview: details.Overview,
            releaseDate: ParseDate(details.ReleaseDate)
        );

        dbContext.Movies.Add(newMovie);
        await dbContext.SaveChangesAsync(cancellationToken);

        // (Optionnel) enrichir immédiatement ; sinon laisse un job asynchrone faire la suite.
        try
        {
            await EnrichMovieAsync(newMovie.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Enrich failed for movie {MovieId} (tmdb {TmdbId})", newMovie.Id, tmdbId);
        }

        return newMovie.Id;
    }

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
}