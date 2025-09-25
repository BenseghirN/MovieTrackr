using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Queries;

public sealed record GetMyListsQuery(Guid UserId) : IRequest<IReadOnlyList<UserListDto>>;

public sealed class GetMyListsHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetMyListsQuery, IReadOnlyList<UserListDto>>
{
    public async Task<IReadOnlyList<UserListDto>> Handle(GetMyListsQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.UserLists
            .Where(l => l.UserId == query.UserId)
            .OrderBy(l => l.Title)
            .ProjectTo<UserListDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}