using FluentValidation;
using MovieTrackR.Application.ReviewLikes.Commands;

namespace MovieTrackR.Application.Validators.ReviewLikes;

public sealed class UnlikeReviewCommandValidator : AbstractValidator<UnlikeReviewCommand>
{
    public UnlikeReviewCommandValidator()
        => RuleFor(x => x.ReviewId).NotEmpty().WithName("id");
}