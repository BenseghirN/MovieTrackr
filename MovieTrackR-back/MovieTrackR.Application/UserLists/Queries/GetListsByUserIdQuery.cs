using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

public sealed record GetListsByUserIdQuery(Guid UserId) : IRequest<IReadOnlyList<UserListDto>>;

public sealed class GetListsByUserIdHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetListsByUserIdQuery, IReadOnlyList<UserListDto>>
{
    public async Task<IReadOnlyList<UserListDto>> Handle(GetListsByUserIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.UserLists
            .Where(l => l.UserId == query.UserId)
            .OrderBy(l => l.Title)
            .ProjectTo<UserListDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}