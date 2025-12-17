
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Genres.Queries;

public sealed record FindGenreByNameQuery(string GenreName) : IRequest<GenreDto?>;

public sealed class FindGenreByNameHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<FindGenreByNameQuery, GenreDto?>
{
    public async Task<GenreDto?> Handle(FindGenreByNameQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Genre> genreQuery = dbContext.Genres.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.GenreName))
        {
            genreQuery = genreQuery.Where(g => g.Name.Contains(query.GenreName.Trim()));
        }

        return await genreQuery
            .ProjectTo<GenreDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}