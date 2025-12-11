using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.ReviewComments.Queries;

public sealed record GetAllCommentsQuery() : IRequest<IReadOnlyList<CommentDto>>;

public sealed class GetAllCommentsHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllCommentsQuery, IReadOnlyList<CommentDto>>
{
    public async Task<IReadOnlyList<CommentDto>> Handle(GetAllCommentsQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.ReviewComments
            .AsNoTracking()
            .ProjectTo<CommentDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}