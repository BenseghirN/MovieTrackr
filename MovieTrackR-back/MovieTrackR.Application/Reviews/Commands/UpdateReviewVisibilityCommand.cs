using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Reviews.Commands;

public sealed record UpdateReviewVisibilityCommand(Guid ReviewId) : IRequest<ReviewListItemDto?>;

public sealed class UpdateReviewVisibilityHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<UpdateReviewVisibilityCommand, ReviewListItemDto?>
{
    public async Task<ReviewListItemDto?> Handle(UpdateReviewVisibilityCommand command, CancellationToken cancellationToken)
    {
        Review review = await dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

        review.SetVisibility(!review.PubliclyVisible);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Reviews
            .AsNoTracking()
            .ProjectTo<ReviewListItemDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId);
    }
}