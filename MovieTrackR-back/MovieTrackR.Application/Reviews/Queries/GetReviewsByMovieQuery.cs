using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetReviewsByMovieQuery(
    Guid MovieId,
    CurrentUserDto? CurrentUser,
    int Page = 1,
    int PageSize = 20,
    MovieReviewSortOption SortOption = MovieReviewSortOption.Newest,
    int? RatingFilter = null) : IRequest<PagedResult<ReviewListItemDto>>;

public sealed class GetReviewsByMovieHandler(IMovieTrackRDbContext dbContext, IMapper mapper, ISender sender)
    : IRequestHandler<GetReviewsByMovieQuery, PagedResult<ReviewListItemDto>>
{
    public async Task<PagedResult<ReviewListItemDto>> Handle(GetReviewsByMovieQuery query, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (query.CurrentUser is not null)
        {
            userId = await sender.Send(new EnsureUserExistsCommand(query.CurrentUser), cancellationToken);
        }

        IQueryable<Review> baseSql = dbContext.Reviews
                    .AsNoTracking()
                    .Where(r => r.MovieId == query.MovieId);

        if (query.RatingFilter is not null)
            baseSql.Where(r => r.Rating == query.RatingFilter);

        baseSql = query.SortOption switch
        {
            MovieReviewSortOption.Newest
                => baseSql.OrderByDescending(r => r.CreatedAt),

            MovieReviewSortOption.Oldest
                => baseSql.OrderBy(r => r.CreatedAt),

            MovieReviewSortOption.HighestRating
                => baseSql.OrderByDescending(r => r.Rating),

            MovieReviewSortOption.LowestRating
                => baseSql.OrderBy(r => r.Rating),

            MovieReviewSortOption.MostCommented
                => baseSql.OrderByDescending(r => r.Comments.Count),

            MovieReviewSortOption.MostLiked
                => baseSql.OrderByDescending(r => r.Likes.Count),

            _ => baseSql.OrderByDescending(r => r.CreatedAt)
        };


        int total = await baseSql.CountAsync(cancellationToken);

        List<Review> reviews = await baseSql
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Include(r => r.Comments)
            .AsSplitQuery()
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        List<ReviewListItemDto> items = mapper.Map<List<Review>, List<ReviewListItemDto>>(
            reviews,
            opts => opts.Items["CurrentUserId"] = userId
        );

        return new PagedResult<ReviewListItemDto>(items, total, query.Page, query.PageSize);
    }
}