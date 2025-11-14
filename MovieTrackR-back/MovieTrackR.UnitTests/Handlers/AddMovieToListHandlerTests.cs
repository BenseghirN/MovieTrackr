using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Movies.Commands;
using MovieTrackR.Application.UserLists.Commands;
using MovieTrackR.Domain.Entities;
using MovieTrackR.UnitTests.Utils;

namespace MovieTrackR.UnitTests.Handlers;

public class AddMovieToListHandlerTests
{
    private static CurrentUserDto FakeUser(string ext = "ext-1", string email = "u@test.dev") =>
        new(ext, email, "User", "User", "Test");

    private static Movie MakeMovie(string title = "Interstellar", int? year = 2014)
        => Movie.CreateNew(
            title: title,
            tmdbId: null,
            originalTitle: null,
            year: year,
            posterUrl: null,
            trailerUrl: null,
            duration: 120,
            overview: "Test movie",
            releaseDate: year is int y ? new DateTime(y, 1, 1) : null
        );

    [Fact]
    public async Task Should_add_movie_with_auto_position_when_not_provided()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        User user = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
        UserList list = UserList.Create(user.Id, "Watchlist", null);
        Movie movie = MakeMovie("Interstellar", 2014);

        db.Users.Add(user);
        db.Movies.Add(movie);
        db.UserLists.Add(list);
        await db.SaveChangesAsync();

        Mock<ISender> sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<EnsureUserExistsCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(user.Id);
        sender.Setup(s => s.Send(It.IsAny<EnsureLocalMovieCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(movie.Id);

        AddMovieToListHandler handler = new AddMovieToListHandler(dbAbs, sender.Object);

        AddMovieToListCommand cmd = new AddMovieToListCommand(
            currentUser: FakeUser("ext-1", "u@test.dev"),
            ListId: list.Id,
            MovieId: movie.Id,
            TmdbId: null,
            Position: null
        );

        await handler.Handle(cmd, CancellationToken.None);

        UserList reloaded = await db.UserLists.Include(l => l.Movies).FirstAsync(l => l.Id == list.Id);
        reloaded.Movies.Should().HaveCount(1);
        reloaded.Movies.First().MovieId.Should().Be(movie.Id);
        reloaded.Movies.First().Position.Should().Be(10);
    }

    [Fact]
    public async Task Should_add_movie_with_explicit_position_when_provided()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        User user = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
        UserList list = UserList.Create(user.Id, "Favs", null);
        Movie movie = MakeMovie("Dune", 2021);

        db.Users.Add(user);
        db.Movies.Add(movie);
        db.UserLists.Add(list);
        await db.SaveChangesAsync();

        Mock<ISender> sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<EnsureUserExistsCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(user.Id);
        sender.Setup(s => s.Send(It.IsAny<EnsureLocalMovieCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(movie.Id);

        AddMovieToListHandler handler = new AddMovieToListHandler(dbAbs, sender.Object);

        AddMovieToListCommand cmd = new AddMovieToListCommand(
            currentUser: FakeUser("ext-2", "x@test.dev"),
            ListId: list.Id,
            MovieId: movie.Id,
            TmdbId: null,
            Position: 42
        );

        await handler.Handle(cmd, CancellationToken.None);

        UserListMovie link = await db.UserListMovies.FirstAsync(x => x.UserListId == list.Id && x.MovieId == movie.Id);
        link.Position.Should().Be(42);
    }

    [Fact]
    public async Task Should_throw_Conflict_when_movie_already_in_list()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        User user = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
        UserList list = UserList.Create(user.Id, "Seen", null);
        Movie movie = MakeMovie("Matrix", 1999);

        list.AddMovie(movie, position: 10);

        db.Users.Add(user);
        db.Movies.Add(movie);
        db.UserLists.Add(list);
        await db.SaveChangesAsync();

        Mock<ISender> sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<EnsureUserExistsCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(user.Id);
        sender.Setup(s => s.Send(It.IsAny<EnsureLocalMovieCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(movie.Id);

        AddMovieToListHandler handler = new AddMovieToListHandler(dbAbs, sender.Object);

        AddMovieToListCommand cmd = new AddMovieToListCommand(
            currentUser: FakeUser("ext-3", "c@test.dev"),
            ListId: list.Id,
            MovieId: movie.Id,
            TmdbId: null,
            Position: null
        );

        Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
                 .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Should_throw_Forbidden_when_list_not_owned_by_user()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        User owner = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
        UserList list = UserList.Create(owner.Id, "Private", null);
        Movie movie = MakeMovie("Seven", 1995);

        db.Users.Add(owner);
        db.Movies.Add(movie);
        db.UserLists.Add(list);
        await db.SaveChangesAsync();

        Mock<ISender> sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<EnsureUserExistsCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Guid.NewGuid()); // ≠ owner.Id
        sender.Setup(s => s.Send(It.IsAny<EnsureLocalMovieCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(movie.Id);

        AddMovieToListHandler handler = new AddMovieToListHandler(dbAbs, sender.Object);

        AddMovieToListCommand cmd = new AddMovieToListCommand(
            currentUser: FakeUser("intr-ext", "i@test.dev"),
            ListId: list.Id,
            MovieId: movie.Id,
            TmdbId: null,
            Position: null
        );

        Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Should_throw_conflict_when_position_already_used_in_list()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        User user = User.Create("ext-1", "u@test.dev", "UserTest", "User", "Test");
        UserList list = UserList.Create(user.Id, "List", null);
        Movie m1 = Movie.CreateNew("A", null, null, 2000, null, null, 100, "a", new DateTime(2000, 1, 1));
        Movie m2 = Movie.CreateNew("B", null, null, 2001, null, null, 100, "b", new DateTime(2001, 1, 1));

        list.AddMovie(m1, 10);
        db.Users.Add(user);
        db.UserLists.Add(list);
        db.Movies.AddRange(m1, m2);
        await db.SaveChangesAsync();

        Mock<ISender> sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<EnsureUserExistsCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(user.Id);
        sender.Setup(s => s.Send(It.IsAny<EnsureLocalMovieCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(m2.Id);

        AddMovieToListHandler handler = new AddMovieToListHandler(dbAbs, sender.Object);
        AddMovieToListCommand cmd = new AddMovieToListCommand(new("ext-1", "u@test.dev", "UserTest", "User", "Test"),
            list.Id, m2.Id, null, 10);

        Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*position*");
    }
}