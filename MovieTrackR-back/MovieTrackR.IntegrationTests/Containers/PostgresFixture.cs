using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace MovieTrackR.IntegrationTests.Containers;

public sealed class PostgresFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
     .WithImage("postgres:16-alpine")
     .WithDatabase("movie_trackr_test")
     .WithUsername("postgres")
     .WithPassword("postgres")
     .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        DbContextOptions<MovieTrackRDbContext> options = new DbContextOptionsBuilder<MovieTrackRDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using MovieTrackRDbContext db = new MovieTrackRDbContext(options);
        await db.Database.MigrateAsync();
    }
    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}