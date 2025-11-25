namespace MovieTrackR.Application.Interfaces;

/// <summary>Service pour peupler les genres depuis TMDB.</summary>
public interface IGenreSeeder
{
    /// <summary>Synchronise les genres TMDB avec la base de données.</summary>
    Task SeedGenresAsync(CancellationToken cancellationToken = default);
}