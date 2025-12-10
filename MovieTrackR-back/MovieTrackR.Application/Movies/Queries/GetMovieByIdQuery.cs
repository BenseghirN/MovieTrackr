using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetMovieByIdQuery(Guid Id) : IRequest<MovieDetailsDto?>;

public sealed class GetMovieByIdHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetMovieByIdQuery, MovieDetailsDto?>
{
    public async Task<MovieDetailsDto?> Handle(GetMovieByIdQuery query, CancellationToken cancellationToken)
    {
        Movie? movie = await dbContext.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.Cast).ThenInclude(mc => mc.Person)
            .Include(m => m.Crew).ThenInclude(mc => mc.Person)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);

        if (movie is null) return null;

        var reviewStats = await dbContext.Reviews
            .Where(r => r.MovieId == query.Id)
            .GroupBy(r => r.MovieId)
            .Select(g => new
            {
                AverageRating = (float?)g.Average(r => r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        MovieDetailsDto dto = mapper.Map<MovieDetailsDto>(movie);
        dto.AverageRating = reviewStats?.AverageRating;
        dto.ReviewCount = reviewStats?.Count ?? 0;
        return dto;
    }
}

