using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Reviews.Commands;

public sealed record CreateReviewCommand(CreateReviewDto Review, CurrentUserDto CurrentUser) : IRequest<Guid>;

public sealed class CreateReviewHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> Handle(CreateReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        bool exists = await dbContext.Reviews
            .AnyAsync(r => r.UserId == userId && r.MovieId == command.Review.MovieId, cancellationToken);
        if (exists) throw new ConflictException("You already reviewed this movie.");

        Review newReview = Review.Create(
            userId,
            command.Review.MovieId,
            command.Review.Rating,
            command.Review.Content
        );

        dbContext.Reviews.Add(newReview);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newReview.Id;
    }
}