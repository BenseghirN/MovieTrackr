using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Queries;

public sealed record GetMyListsQuery(Guid UserId) : IRequest<IReadOnlyList<UserListDto>>;

public sealed class GetMyListsHandler(IMovieTrackRDbContext db, IMapper mapper)
    : IRequestHandler<GetMyListsQuery, IReadOnlyList<UserListDto>>
{
    public async Task<IReadOnlyList<UserListDto>> Handle(GetMyListsQuery r, CancellationToken ct)
    {
        return await db.UserLists
            .Where(l => l.UserId == r.UserId)
            .OrderBy(l => l.Title)
            .ProjectTo<UserListDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}