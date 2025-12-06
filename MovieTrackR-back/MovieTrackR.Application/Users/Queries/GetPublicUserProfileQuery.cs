using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Users.Queries;

public sealed record GetPublicUserProfileQuery(Guid Id) : IRequest<PublicUserProfileDto?>;

internal sealed class GetPublicUserProfileHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetPublicUserProfileQuery, PublicUserProfileDto?>
{
    public async Task<PublicUserProfileDto?> Handle(GetPublicUserProfileQuery query, CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == query.Id)
            .ProjectTo<PublicUserProfileDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
}