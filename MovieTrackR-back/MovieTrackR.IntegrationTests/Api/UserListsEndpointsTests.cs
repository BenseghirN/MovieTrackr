using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Infrastructure.Persistence;
using MovieTrackR.IntegrationTests.Containers;
using MovieTrackR.IntegrationTests.Utils;

namespace MovieTrackR.IntegrationTests.Api;

public sealed class UserListsEndpointsTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly TestAppFactory _factory;
    private readonly HttpClient _client;

    public UserListsEndpointsTests(PostgresFixture fixture)
    {
        _factory = new TestAppFactory(fixture);
        _client = _factory.CreateClientAuthenticated();
    }

    public async Task InitializeAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        MovieTrackRDbContext db = scope.ServiceProvider.GetRequiredService<MovieTrackRDbContext>();
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
    public async Task AddMovie_returns_Conflict_when_position_already_used()
    {
        Guid listId;
        Guid m1Id, m2Id;

        using (var scope = _factory.Services.CreateScope())
        {
            MovieTrackRDbContext db = scope.ServiceProvider.GetRequiredService<MovieTrackRDbContext>();

            User user = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            UserList list = UserList.Create(user.Id, "My List", null);
            db.UserLists.Add(list);
            await db.SaveChangesAsync();

            Movie m1 = Movie.CreateNew("A", null, null, 2000, null, null, null, null, 100, "a", new(2000, 1, 1), null);
            Movie m2 = Movie.CreateNew("B", null, null, 2001, null, null, null, null, 100, "b", new(2001, 1, 1), null);
            db.Movies.AddRange(m1, m2);
            await db.SaveChangesAsync();

            list.AddMovie(m1, 10);
            await db.SaveChangesAsync();

            listId = list.Id;
            m1Id = m1.Id;
            m2Id = m2.Id;
        }

        var payload = new
        {
            listId,
            movieId = m2Id,
            tmdbId = (int?)null,
            position = 10
        };

        HttpResponseMessage res = await _client.PostAsJsonAsync($"/api/v1/me/lists/{listId}/movie", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddMovie_succeeds_with_auto_position_when_not_provided()
    {
        Guid listId;
        Guid mId;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            MovieTrackRDbContext db = scope.ServiceProvider.GetRequiredService<MovieTrackRDbContext>();

            User user = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            UserList list = UserList.Create(user.Id, "Watchlist", null);
            db.UserLists.Add(list);

            Movie m = Movie.CreateNew("Interstellar", 157336, "Interstellar", 2014, null, null, null, null, 169, "test", new(2014, 11, 7), null);
            db.Movies.Add(m);

            await db.SaveChangesAsync();

            listId = list.Id;
            mId = m.Id;
        }

        var payload = new
        {
            movieId = mId,
            tmdbId = (int?)null,
            position = (int?)null
        };

        HttpResponseMessage res = await _client.PostAsJsonAsync($"/api/v1/me/lists/{listId}/movie", payload);
        res.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.Created);
    }
}