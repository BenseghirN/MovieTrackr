using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Queries;

public sealed record GetListByIdQuery(CurrentUserDto currentUser, Guid ListId) : IRequest<UserListDetailsDto?>;
public sealed class GetListByIdHandler(IMovieTrackRDbContext dbContext, IMapper mapper, ISender sender)
    : IRequestHandler<GetListByIdQuery, UserListDetailsDto?>
{
    public async Task<UserListDetailsDto?> Handle(GetListByIdQuery query, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(query.currentUser), cancellationToken);

        return await dbContext.UserLists
            .Where(l => l.Id == query.ListId && l.UserId == userId)
            .ProjectTo<UserListDetailsDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}