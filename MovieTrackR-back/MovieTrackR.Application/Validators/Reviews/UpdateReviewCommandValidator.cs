using FluentValidation;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("id");
        RuleFor(x => x.UpdatedReview.Rating).InclusiveBetween(0, 10).WithName("rating");
        RuleFor(x => x.UpdatedReview.Content).MaximumLength(4000).WithName("content");
    }
}