using FluentValidation;
using MovieTrackR.Application.Reviews.Commands;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Review.MovieId).NotEmpty().WithName("movieId");
        RuleFor(x => x.Review.Rating).InclusiveBetween(0, 10).WithName("rating");
        RuleFor(x => x.Review.Content).MaximumLength(4000).WithName("content");
    }
}