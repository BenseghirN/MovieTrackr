using FluentValidation;
using MovieTrackR.Application.Reviews.Commands;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("id");
    }
}