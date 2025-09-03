using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetMovieByIdQuery(Guid Id) : IRequest<MovieDto?>;

public sealed class GetMovieByIdHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetMovieByIdQuery, MovieDto?>
{
    public async Task<MovieDto?> Handle(GetMovieByIdQuery q, CancellationToken ct)
    {
        Movie? movie = await dbContext.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == q.Id, ct);

        return movie is null ? null : mapper.Map<MovieDto>(movie);
    }
}

