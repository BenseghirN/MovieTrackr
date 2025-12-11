using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.People.Queries;

public sealed record GetAllPeopleQuery()
    : IRequest<IReadOnlyList<PersonDetailsDto>>;

public sealed class GetAllPeopleHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllPeopleQuery, IReadOnlyList<PersonDetailsDto>>
{
    public async Task<IReadOnlyList<PersonDetailsDto>> Handle(GetAllPeopleQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.People
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ProjectTo<PersonDetailsDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}