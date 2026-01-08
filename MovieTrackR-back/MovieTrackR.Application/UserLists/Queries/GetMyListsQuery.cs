using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Queries;

public sealed record GetMyListsQuery(CurrentUserDto currentUser) : IRequest<IReadOnlyList<UserListDto>>;

public sealed class GetMyListsHandler(IMovieTrackRDbContext dbContext, IMapper mapper, IMediator mediator)
    : IRequestHandler<GetMyListsQuery, IReadOnlyList<UserListDto>>
{
    public async Task<IReadOnlyList<UserListDto>> Handle(GetMyListsQuery query, CancellationToken cancellationToken)
    {
        Guid userId = await mediator.Send(new EnsureUserExistsCommand(query.currentUser), cancellationToken);

        return await dbContext.UserLists
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Title)
            .ProjectTo<UserListDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}