using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetAllReviewsQuery() : IRequest<IReadOnlyList<ReviewListItemDto>>;

public sealed class GetAllReviewsHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllReviewsQuery, IReadOnlyList<ReviewListItemDto>>
{
    public async Task<IReadOnlyList<ReviewListItemDto>> Handle(GetAllReviewsQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .ProjectTo<ReviewListItemDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}