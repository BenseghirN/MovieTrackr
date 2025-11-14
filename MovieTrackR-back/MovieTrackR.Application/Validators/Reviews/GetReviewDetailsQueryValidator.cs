using FluentValidation;
using MovieTrackR.Application.Reviews.Queries;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class GetReviewDetailsQueryValidator : AbstractValidator<GetReviewDetailsQuery>
{
    public GetReviewDetailsQueryValidator()
        => RuleFor(x => x.ReviewId).NotEmpty().WithName("id");
}