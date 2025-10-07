using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

public sealed record UpdateReviewCommand(Guid ReviewId, UpdateReviewDto UpdatedReview, CurrentUserDto CurrentUser) : IRequest;

public sealed class UpdateReviewHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<UpdateReviewCommand>
{
    public async Task Handle(UpdateReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        Review review = await dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

        if (review.UserId != userId)
            throw new ForbiddenException();

        review.Update(command.UpdatedReview.Rating, command.UpdatedReview.Content);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}