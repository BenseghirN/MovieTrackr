using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.Reviews.Queries;

public sealed record GetReviewDetailsQuery(Guid ReviewId) : IRequest<ReviewDetailsDto>;

public sealed class GetReviewDetailsHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetReviewDetailsQuery, ReviewDetailsDto>
{
    public async Task<ReviewDetailsDto> Handle(GetReviewDetailsQuery query, CancellationToken cancellationToken)
    {
        ReviewDetailsDto? review = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.Id == query.ReviewId)
            .ProjectTo<ReviewDetailsDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return review ?? throw new NotFoundException("Review not found.");
    }
}