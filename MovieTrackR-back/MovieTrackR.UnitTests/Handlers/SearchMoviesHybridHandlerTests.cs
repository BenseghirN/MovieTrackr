using AutoMapper;
using FluentAssertions;
using Moq;
using MovieTrackR.Application.Mapping;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.Movies.Queries;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.UnitTests.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.UnitTests.Handlers;

public class SearchMoviesHybridHandlerTests
{
    private readonly IMapper _mapper;

    public SearchMoviesHybridHandlerTests()
    {
        MapperConfigurationExpression expr = new MapperConfigurationExpression();
        expr.AddProfile<MappingProfile>();

        MapperConfiguration config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    private static Movie MakeMovie(string title, int? year = null, int? tmdbId = null)
        => Movie.CreateNew(
            title: title, tmdbId: tmdbId, originalTitle: null,
            year: year, posterUrl: null, trailerUrl: null,
            duration: 120, overview: "test", releaseDate: year is int y ? new DateTime(y, 1, 1) : null);

    [Fact]
    public async Task Should_merge_local_and_tmdb_results()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        db.Movies.Add(MakeMovie("Interstellar", 2014, tmdbId: 157336));
        db.Movies.Add(MakeMovie("Dune", 2021));
        await db.SaveChangesAsync();

        Mock<ITmdbClient> tmdbMock = new Mock<ITmdbClient>();
        tmdbMock
            .Setup(c => c.SearchMoviesAsync("Interstellar", 1, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmdbSearchMoviesResponse(
                Page: 1,
                TotalResults: 2,
                TotalPages: 1,
                Results: new List<TmdbSearchMovieItem>
                {
                    new(157336, "Interstellar", "Interstellar", "2014-11-05", "/p1.jpg", 8.6, 1000), // doublon de local
                    new(27205,   "Inception",   "Inception",   "2010-07-16", "/p2.jpg", 8.7, 2000)  // nouveau
                }
            ));

        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdbMock.Object, _mapper);
        MovieSearchCriteria criteria = new MovieSearchCriteria
        {
            Query = "Interstellar",
            Page = 1,
            PageSize = 10
        };

        HybridPagedResult<SearchMovieResultDto> result = await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Meta.TotalLocal.Should().Be(1);
        result.Meta.TotalTmdb.Should().Be(2);
        result.Meta.TotalResults.Should().Be(3);
        result.Meta.TotalTmdbPages.Should().Be(1);
        result.Meta.HasMore.Should().BeFalse();

        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title).Should().BeEquivalentTo(new[] { "Interstellar", "Inception" });
    }

    [Fact]
    public async Task Should_return_only_local_and_not_call_tmdb_when_query_is_empty()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();
        db.Movies.AddRange(
            MakeMovie("A", 2000),
            MakeMovie("B", 2001),
            MakeMovie("C", 2002)
        );
        await db.SaveChangesAsync();

        Mock<ITmdbClient> tmdbMock = new Mock<ITmdbClient>(MockBehavior.Strict);
        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdbMock.Object, _mapper);

        MovieSearchCriteria criteria = new MovieSearchCriteria
        {
            Query = null,
            Page = 1,
            PageSize = 10
        };

        HybridPagedResult<SearchMovieResultDto> result = await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Meta.TotalLocal.Should().Be(3);
        result.Meta.TotalTmdb.Should().Be(0);
        result.Meta.TotalResults.Should().Be(3);
        result.Meta.TotalTmdbPages.Should().BeNull();  // pas d’appel TMDb
        result.Meta.HasMore.Should().BeFalse();

        result.Items.Should().HaveCount(3);
        tmdbMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasMore_should_be_true_when_either_local_or_tmdb_has_more()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();
        db.Movies.AddRange(
            MakeMovie("A"), MakeMovie("B"), MakeMovie("C"),
            MakeMovie("D"), MakeMovie("E")
        );
        await db.SaveChangesAsync();

        Mock<ITmdbClient> tmdbMock = new Mock<ITmdbClient>();
        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdbMock.Object, _mapper);
        MovieSearchCriteria criteria = new MovieSearchCriteria { Query = null, Page = 1, PageSize = 3 };

        HybridPagedResult<SearchMovieResultDto> result = await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Meta.TotalLocal.Should().Be(5);
        result.Meta.TotalResults.Should().Be(5);
        result.Meta.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task Should_return_only_tmdb_when_no_local_matches()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();
        db.Movies.Add(MakeMovie("Interstellar", 2014, tmdbId: 157336)); // bruit, != "Blade Runner"
        await db.SaveChangesAsync();

        Mock<ITmdbClient> tmdbMock = new Mock<ITmdbClient>();
        tmdbMock
            .Setup(c => c.SearchMoviesAsync("Blade Runner", 1, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmdbSearchMoviesResponse(
                Page: 1,
                TotalResults: 2,
                TotalPages: 1,
                Results: new List<TmdbSearchMovieItem>
                {
                new(78, "Blade Runner", "Blade Runner", "1982-06-25", "/p1.jpg", 8.1, 900),
                new(335984, "Blade Runner 2049", "Blade Runner 2049", "2017-10-04", "/p2.jpg", 7.9, 1200)
                }
            ));

        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdbMock.Object, _mapper);
        MovieSearchCriteria criteria = new MovieSearchCriteria { Query = "Blade Runner", Page = 1, PageSize = 10 };

        HybridPagedResult<SearchMovieResultDto> result = await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Meta.TotalLocal.Should().Be(0);
        result.Meta.TotalTmdb.Should().Be(2);
        result.Meta.TotalResults.Should().Be(2);
        result.Meta.TotalTmdbPages.Should().Be(1);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title).Should().BeEquivalentTo(new[] { "Blade Runner", "Blade Runner 2049" });

        // Optionnel : vérifier que ce sont bien des éléments TMDb (pas locaux)
        result.Items.All(i => i.LocalId is null).Should().BeTrue();
    }

    [Fact]
    public async Task Should_paginate_locals_only_on_page_2_when_no_search_term()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        db.Movies.AddRange(
            MakeMovie("A"), MakeMovie("B"), MakeMovie("C"),
            MakeMovie("D"), MakeMovie("E"), MakeMovie("F"),
            MakeMovie("G")
        );
        await db.SaveChangesAsync();

        Mock<ITmdbClient> tmdbMock = new Mock<ITmdbClient>(MockBehavior.Strict);
        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdbMock.Object, _mapper);
        MovieSearchCriteria criteria = new MovieSearchCriteria { Query = null, Page = 2, PageSize = 3 };

        HybridPagedResult<SearchMovieResultDto> result = await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Meta.TotalLocal.Should().Be(7);
        result.Meta.TotalTmdb.Should().Be(0);
        result.Meta.TotalResults.Should().Be(7);
        result.Meta.TotalTmdbPages.Should().BeNull();
        result.Meta.HasMore.Should().BeTrue();

        result.Items.Should().HaveCount(3);
        result.Items.Select(i => i.Title).Should().Equal("D", "E", "F");

        tmdbMock.VerifyNoOtherCalls();
    }
}