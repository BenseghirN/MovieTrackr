using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.ReviewLikes.Commands;

public sealed record UnlikeReviewCommand(Guid ReviewId, CurrentUserDto CurrentUser) : IRequest;

public sealed class UnlikeReviewHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<UnlikeReviewCommand>
{
    public async Task Handle(UnlikeReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        ReviewLike? like = await dbContext.ReviewLikes
            .FirstOrDefaultAsync(l => l.ReviewId == command.ReviewId && l.UserId == userId, cancellationToken);

        if (like != null)
        {
            dbContext.ReviewLikes.Remove(like);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}