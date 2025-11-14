using FluentValidation;
using MovieTrackR.Application.ReviewLikes.Commands;

namespace MovieTrackR.Application.Validators.ReviewLikes;

public sealed class LikeReviewCommandValidator : AbstractValidator<LikeReviewCommand>
{
    public LikeReviewCommandValidator()
        => RuleFor(x => x.ReviewId).NotEmpty().WithName("id");
}
