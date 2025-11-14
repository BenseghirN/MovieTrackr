using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetReviewsByUserQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReviewListItemDto>>;

public sealed class GetReviewsByUserHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetReviewsByUserQuery, PagedResult<ReviewListItemDto>>
{
    public async Task<PagedResult<ReviewListItemDto>> Handle(GetReviewsByUserQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Review> baseSql = dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == query.UserId);

        int total = await baseSql.CountAsync(cancellationToken);

        List<ReviewListItemDto> items = await baseSql
            .OrderByDescending(r => r.CreatedAt)
            .ProjectTo<ReviewListItemDto>(mapper.ConfigurationProvider)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ReviewListItemDto>(items, total, query.Page, query.PageSize);
    }
}