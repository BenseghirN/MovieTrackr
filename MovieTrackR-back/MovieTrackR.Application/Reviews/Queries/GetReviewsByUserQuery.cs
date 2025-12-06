using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetReviewsByUserQuery(
    Guid UserId, int Page = 1,
    int PageSize = 20,
    UserReviewSortOption Sort = UserReviewSortOption.Newest,
    int? RatingFilter = null) : IRequest<PagedResult<ReviewListItemDto>>;

public sealed class GetReviewsByUserHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetReviewsByUserQuery, PagedResult<ReviewListItemDto>>
{
    public async Task<PagedResult<ReviewListItemDto>> Handle(GetReviewsByUserQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Review> baseSql = dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == query.UserId);

        if (query.RatingFilter is not null)
            baseSql = baseSql.Where(r => r.Rating == query.RatingFilter);

        baseSql = query.Sort switch
        {
            UserReviewSortOption.Newest
                => baseSql.OrderByDescending(r => r.CreatedAt),

            UserReviewSortOption.Oldest
                => baseSql.OrderBy(r => r.CreatedAt),

            UserReviewSortOption.HighestRating
                => baseSql.OrderByDescending(r => r.Rating),

            UserReviewSortOption.LowestRating
                => baseSql.OrderBy(r => r.Rating),

            _ => baseSql.OrderByDescending(r => r.CreatedAt)
        };

        int total = await baseSql.CountAsync(cancellationToken);

        List<ReviewListItemDto> items = await baseSql
            .ProjectTo<ReviewListItemDto>(mapper.ConfigurationProvider)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ReviewListItemDto>(items, total, query.Page, query.PageSize);
    }
}