using FluentValidation;
using MovieTrackR.Application.Reviews.Commands;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Review.MovieId).NotEmpty().WithName("movieId");
        RuleFor(x => x.Review.Rating).InclusiveBetween(0, 5).WithMessage("Rating is out of scope");
        RuleFor(x => x.Review.Content).MaximumLength(5000).WithMessage("Review content is too long.");
        RuleFor(x => x.Review.Content).NotEmpty().WithMessage("Review content cannot be empty.");
    }
}