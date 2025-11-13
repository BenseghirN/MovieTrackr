using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Infrastructure.Persistence;

namespace MovieTrackR.UnitTests.Utils;

public static class InMemoryDbContextFactory
{
    public static (IMovieTrackRDbContext Abstraction, MovieTrackRDbContext Concrete) Create(string? name = null)
    {
        string dbName = string.IsNullOrWhiteSpace(name) ? Guid.NewGuid().ToString() : name;

        DbContextOptions<MovieTrackRDbContext> options = new DbContextOptionsBuilder<MovieTrackRDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        MovieTrackRDbContext concrete = new MovieTrackRDbContext(options);
        IMovieTrackRDbContext abstraction = concrete;
        return (abstraction, concrete);
    }
}