using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.TMDB.Interfaces;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetStreamingOffersForMovieQuery(int MovieTmdbId, string? CountryCode)
    : IRequest<StreamingOfferDto?>;

public sealed class GetStreamingOffersForMovieHandler(ITmdbCatalogService tmdbCatalogService)
    : IRequestHandler<GetStreamingOffersForMovieQuery, StreamingOfferDto?>
{
    public async Task<StreamingOfferDto?> Handle(GetStreamingOffersForMovieQuery query, CancellationToken cancellationToken)
    {
        string country = string.IsNullOrWhiteSpace(query.CountryCode)
            ? "BE"
            : query.CountryCode.ToUpperInvariant();

        return await tmdbCatalogService.GetMovieStreamingOffersAsync(query.MovieTmdbId, country, cancellationToken);
    }
}