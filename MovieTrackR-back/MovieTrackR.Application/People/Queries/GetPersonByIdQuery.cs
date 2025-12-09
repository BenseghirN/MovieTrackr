using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.People.Queries;

public sealed record GetPersonByIdQuery(Guid PersonId)
    : IRequest<PersonDetailsDto?>;

public sealed class GetPersonByIdHandler(IMovieTrackRDbContext dbContext, ITmdbCatalogService catalogService, IMapper mapper)
    : IRequestHandler<GetPersonByIdQuery, PersonDetailsDto?>
{
    public async Task<PersonDetailsDto?> Handle(GetPersonByIdQuery query, CancellationToken cancellationToken)
    {
        Person? person = await dbContext.People
            .AsNoTracking()
            .Include(p => p.CastRoles).ThenInclude(c => c.Movie)
            .Include(p => p.CrewRoles).ThenInclude(c => c.Movie)
            .FirstOrDefaultAsync(p => p.Id == query.PersonId, cancellationToken);

        if (person is null) return null;

        if (string.IsNullOrWhiteSpace(person.Biography) && person.TmdbId.HasValue)
        {
            Guid personId = await catalogService.EnsurePersonExistsAsync(person.TmdbId.Value, cancellationToken);
            person = await dbContext.People.FirstOrDefaultAsync(p => p.Id == personId, cancellationToken);
        }

        PersonDetailsDto dto = mapper.Map<PersonDetailsDto>(person);
        return dto;
    }
}