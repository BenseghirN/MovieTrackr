using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Infrastructure.Persistence;
using MovieTrackR.IntegrationTests.Containers;
using MovieTrackR.IntegrationTests.Utils;

namespace MovieTrackR.IntegrationTests.Api;

public sealed class MoviesEndpointsTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly TestAppFactory _factory;
    private readonly HttpClient _client;

    public MoviesEndpointsTests(PostgresFixture pg)
    {
        _factory = new TestAppFactory(pg);
        _client = _factory.CreateClientAuthenticated();
    }
    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MovieTrackRDbContext>();

        await db.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE 
                user_list_movies, user_lists, 
                review_comments, review_likes, reviews, 
                movie_genres, movie_cast, movie_crew, 
                movies, genres, people, 
                movie_proposals, users 
            RESTART IDENTITY CASCADE
        ");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Search_merges_local_and_tmdb_dedupes_and_returns_200()
    {
        // Seed 1 film local: Interstellar (tmdbId = 157336)
        using (var scope = _factory.Services.CreateScope())
        {
            MovieTrackRDbContext db = scope.ServiceProvider.GetRequiredService<MovieTrackRDbContext>();
            db.Movies.Add(Movie.CreateNew("Interstellar", 157336, "Interstellar", 2014, null, null, null, null, 169, "test", new(2014, 11, 7), null));
            await db.SaveChangesAsync();
        }

        // TMDb: 1 doublon + 1 différent
        _factory.TmdbMock
            .Setup(c => c.SearchMoviesAsync(It.Is<MovieSearchCriteria>(crit => crit.Query == "Inter"), "fr-FR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmdbSearchMoviesResponse(
                Page: 1, TotalResults: 2, TotalPages: 1,
                Results: new[]
                {
                    new TmdbSearchMovieItem(157336, "Interstellar", "Interstellar", "2014-11-07", "/p1.jpg", 8.6, 1000),
                    new TmdbSearchMovieItem(999999, "Interstate 60", "Interstate 60", "2002-04-13", "/x.jpg", 7.0, 10),
                }
            ));

        HttpResponseMessage res = await _client.GetAsync("/api/v1/movies/search?Query=Inter&Page=1&PageSize=20");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        JsonElement items = doc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().Be(2); // local Interstellar + tmdb Interstate 60

        string?[] titles = items.EnumerateArray().Select(e => e.GetProperty("title").GetString()).ToArray();
        titles.Should().Contain(new[] { "Interstellar", "Interstate 60" });

        _factory.TmdbMock.VerifyAll();
    }

    [Fact]
    public async Task Search_with_empty_query_returns_only_locals_and_does_not_call_tmdb()
    {
        // Seed quelques films locaux
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieTrackRDbContext>();
            db.Movies.AddRange(
                Movie.CreateNew("A", null, null, 2000, null, null, null, null, 100, "a", new(2000, 1, 1), null),
                Movie.CreateNew("B", null, null, 2001, null, null, null, null, 100, "b", new(2001, 1, 1), null)
            );
            await db.SaveChangesAsync();
        }

        var res = await _client.GetAsync("/api/v1/movies/search?Page=1&PageSize=20"); // Search (query) absente
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("\"title\":\"A\"").And.Contain("\"title\":\"B\"");
    }
}