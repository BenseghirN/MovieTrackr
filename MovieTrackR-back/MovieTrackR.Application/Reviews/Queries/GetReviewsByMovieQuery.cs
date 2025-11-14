using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetReviewsByMovieQuery(Guid MovieId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReviewListItemDto>>;

public sealed class GetReviewsByMovieHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetReviewsByMovieQuery, PagedResult<ReviewListItemDto>>
{
    public async Task<PagedResult<ReviewListItemDto>> Handle(GetReviewsByMovieQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Review> baseSql = dbContext.Reviews.Where(r => r.MovieId == query.MovieId);
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