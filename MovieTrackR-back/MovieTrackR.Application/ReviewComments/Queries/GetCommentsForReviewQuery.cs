using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.ReviewComments.Queries;

public sealed record GetCommentsForReviewQuery(Guid ReviewId, int Page = 1, int PageSize = 50) : IRequest<PagedResult<CommentDto>>;

public sealed class GetCommentsForReviewHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetCommentsForReviewQuery, PagedResult<CommentDto>>
{
    public async Task<PagedResult<CommentDto>> Handle(GetCommentsForReviewQuery query, CancellationToken ct)
    {
        IQueryable<Domain.Entities.ReviewComment> baseSql = dbContext.ReviewComments
            .AsNoTracking()
            .Where(c => c.ReviewId == query.ReviewId);

        int total = await baseSql.CountAsync(ct);

        List<CommentDto> items = await baseSql
            .OrderBy(c => c.CreatedAt)
            .ProjectTo<CommentDto>(mapper.ConfigurationProvider)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<CommentDto>(items, total, query.Page, query.PageSize);
    }
}