using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Queries;

public sealed record GetListByIdQuery(Guid UserId, Guid ListId) : IRequest<UserListDetailsDto?>;
public sealed class GetListByIdHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetListByIdQuery, UserListDetailsDto?>
{
    public async Task<UserListDetailsDto?> Handle(GetListByIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.UserLists
            .Where(l => l.Id == query.ListId && l.UserId == query.UserId)
            .ProjectTo<UserListDetailsDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}