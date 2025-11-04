using FluentAssertions;
using Moq;
using MovieTrackR.Application.Movies.Commands;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.UnitTests.Utils;

namespace MovieTrackR.UnitTests.Handlers;

public class EnsureLocalMovieCommandHandlerTests
{
    [Fact]
    public async Task Returns_movieId_when_provided_locally()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        Movie movie = Movie.CreateNew("Interstellar", null, null, 2014, null, null, 120, "Test", new DateTime(2014, 1, 1));
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        Mock<ITmdbCatalogService> catalog = new(MockBehavior.Strict);
        EnsureLocalMovieHandler handler = new(dbAbs, catalog.Object);

        EnsureLocalMovieCommand cmd = new(MovieId: movie.Id, TmdbId: null);

        Guid result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().Be(movie.Id);
        catalog.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Imports_from_tmdb_when_tmdbId_provided()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        Guid importedId = Guid.NewGuid();
        Mock<ITmdbCatalogService> catalog = new();
        catalog.Setup(s => s.ImportMovieAsync(157336, It.IsAny<CancellationToken>()))
               .ReturnsAsync(importedId);

        EnsureLocalMovieHandler handler = new(dbAbs, catalog.Object);
        EnsureLocalMovieCommand cmd = new(MovieId: null, TmdbId: 157336);

        Guid result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().Be(importedId);
        catalog.Verify(s => s.ImportMovieAsync(157336, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Throws_when_both_ids_missing()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        Mock<ITmdbCatalogService> catalog = new(MockBehavior.Strict);
        EnsureLocalMovieHandler handler = new(dbAbs, catalog.Object);

        EnsureLocalMovieCommand cmd = new(MovieId: null, TmdbId: null);

        Func<Task<Guid>> act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*MovieId*TmdbId*");
    }

    [Fact]
    public async Task Should_return_existing_local_id_and_not_import_when_tmdbid_already_in_db()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        Movie existing = Movie.CreateNew("Inception", 27205, "Inception", 2010, null, null, 148, "dream", new DateTime(2010, 7, 16));
        db.Movies.Add(existing);
        await db.SaveChangesAsync();

        Mock<ITmdbCatalogService> catalog = new Mock<ITmdbCatalogService>(MockBehavior.Strict);
        EnsureLocalMovieHandler handler = new EnsureLocalMovieHandler(dbAbs, catalog.Object);

        EnsureLocalMovieCommand cmd = new EnsureLocalMovieCommand(MovieId: null, TmdbId: 27205);
        Guid id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().Be(existing.Id);
        catalog.VerifyNoOtherCalls();
    }
}