using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetReviewsByMovieQuery(Guid MovieId, CurrentUserDto? CurrentUser, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReviewListItemDto>>;

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
                    .Where(r => r.MovieId == query.MovieId)
                    .Include(r => r.User)
                    .Include(r => r.Likes)
                    .Include(r => r.Comments)
                    .AsSplitQuery();

        int total = await baseSql.CountAsync(cancellationToken);

        List<Review> reviews = await baseSql
            .OrderByDescending(r => r.CreatedAt)
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