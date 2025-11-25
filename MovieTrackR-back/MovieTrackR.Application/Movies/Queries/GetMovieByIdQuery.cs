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
            .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);

        return movie is null ? null : mapper.Map<MovieDetailsDto>(movie);
    }
}

