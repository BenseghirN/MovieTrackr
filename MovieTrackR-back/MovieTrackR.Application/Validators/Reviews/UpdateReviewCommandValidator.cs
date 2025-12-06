using FluentValidation;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("id");
        RuleFor(x => x.UpdatedReview.Rating).InclusiveBetween(0, 5).WithMessage("Rating is out of scope");
        RuleFor(x => x.UpdatedReview.Content).MaximumLength(5000).WithMessage("Review content is too long.");
        RuleFor(x => x.UpdatedReview.Content).NotEmpty().WithMessage("Review content cannot be empty.");
    }
}