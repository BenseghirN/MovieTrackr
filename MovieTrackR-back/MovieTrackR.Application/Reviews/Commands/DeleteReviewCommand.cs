using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Reviews.Commands;

public sealed record DeleteReviewCommand(Guid ReviewId, CurrentUserDto CurrentUser) : IRequest;

public sealed class DeleteReviewHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(DeleteReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        Review review = await dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

        if (review.UserId != userId)
            throw new ForbiddenException();

        dbContext.Reviews.Remove(review);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}