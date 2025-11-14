using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.ReviewLikes.Commands;

public sealed record LikeReviewCommand(Guid ReviewId, CurrentUserDto CurrentUser) : IRequest;

public sealed class LikeReviewHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<LikeReviewCommand>
{
    public async Task Handle(LikeReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        Review review = await dbContext.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

        bool exists = await dbContext.ReviewLikes
            .AnyAsync(l => l.ReviewId == command.ReviewId && l.UserId == userId, cancellationToken);
        if (!exists)
        {
            ReviewLike like = ReviewLike.Create(userId, command.ReviewId);
            dbContext.ReviewLikes.Add(like);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}