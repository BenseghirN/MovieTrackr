using AutoMapper;
using FluentAssertions;
using Moq;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Mapping;
using MovieTrackR.Application.Movies;
using MovieTrackR.Application.Movies.Queries;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.UnitTests.Utils;
using Microsoft.Extensions.Logging.Abstractions;
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

    [Fact]
    public async Task Should_merge_local_and_tmdb_results()
    {
        var (dbAbs, db) = InMemoryDbContextFactory.Create();

        Movie local = Movie.CreateNew(
            title: "Blade Runner",
            tmdbId: 78,
            originalTitle: "Blade Runner",
            year: 1982,
            posterUrl: null,
            trailerUrl: null,
            duration: 117,
            overview: "Neo-noir SF",
            releaseDate: new DateTime(1982, 6, 25)
        );
        db.Movies.Add(local);
        await db.SaveChangesAsync();

        Mock<ITmdbClient> tmdb = new Mock<ITmdbClient>();
        tmdb.Setup(c => c.SearchMoviesAsync(
                    "Blade", 1, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmdbSearchMoviesResponse(
                Page: 1,
                TotalResults: 1,
                TotalPages: 1,
                Results: new[]
                {
                    new TmdbSearchMovieItem(
                        Id: 335984,
                        Title: "Blade Runner 2049",
                        OriginalTitle: "Blade Runner 2049",
                        ReleaseDate: "2017-10-04",
                        PosterPath: "/p.jpg",
                        VoteAverage: 7.8,
                        Popularity: 20.0
                    )
                }.ToList()
            ));

        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdb.Object, _mapper);

        MovieSearchCriteria criteria = new MovieSearchCriteria
        {
            Query = "Blade",
            Page = 1,
            PageSize = 20
        };

        HybridPagedResult<SearchMovieResultDto> result =
            await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Meta.TotalLocal.Should().Be(1);
        result.Meta.TotalResults.Should().Be(1);
        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Title)
              .Should().Contain(new[] { "Blade Runner", "Blade Runner 2049" });
    }

    [Fact]
    public async Task Should_return_only_tmdb_when_no_local_matches()
    {
        var (dbAbs, _) = InMemoryDbContextFactory.Create();

        Mock<ITmdbClient> tmdb = new Mock<ITmdbClient>();
        tmdb.Setup(x => x.SearchMoviesAsync("Dune", 1, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmdbSearchMoviesResponse(
                Page: 1,
                TotalResults: 1,
                TotalPages: 1,
                Results: new List<TmdbSearchMovieItem>
                {
                    new(
                        Id: 693134,
                        Title: "Dune: Part Two",
                        OriginalTitle: "Dune: Part Two",
                        ReleaseDate: "2024-02-29",
                        PosterPath: "/dune2.jpg",
                        VoteAverage: 8.3,
                        Popularity: 250.5
                    )
                }
            ));

        SearchMoviesHybridHandler handler = new SearchMoviesHybridHandler(dbAbs, tmdb.Object, _mapper);
        MovieSearchCriteria criteria = new MovieSearchCriteria { Query = "Dune", Page = 1, PageSize = 10 };

        HybridPagedResult<SearchMovieResultDto> result = await handler.Handle(new SearchMoviesHybridQuery(criteria), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Dune: Part Two");
        result.Items[0].IsLocal.Should().BeFalse();
        result.Meta.TotalLocal.Should().Be(0);
        result.Meta.TotalResults.Should().Be(1);
    }
}