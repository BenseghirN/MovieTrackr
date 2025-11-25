using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Infrastructure.TMDB.Services;

public sealed class GenreSeeder(
    IMovieTrackRDbContext dbContext,
    ITmdbClient tmdbClient,
    ILogger<GenreSeeder> logger) : IGenreSeeder
{
    public async Task SeedGenresAsync(CancellationToken cancellationToken = default)
    {
        bool hasGenres = await dbContext.Genres.AnyAsync(cancellationToken);
        if (hasGenres)
        {
            logger.LogInformation("Genres already seeded, skipping");
            return;
        }

        logger.LogInformation("Fetching genres from TMDB...");

        TmdbGenresResponse tmdbGenres = await tmdbClient.GetGenresAsync("fr-FR", cancellationToken);

        if (tmdbGenres.Genres.Count == 0)
        {
            logger.LogWarning("No genres returned from TMDB");
            return;
        }

        List<Genre> genres = tmdbGenres.Genres
            .Select(g => Genre.Create(g.Id, g.Name))
            .ToList();

        await dbContext.Genres.AddRangeAsync(genres, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} genres from TMDB", genres.Count);
    }
}