using FluentValidation;
using MovieTrackR.Application.ReviewComments.Queries;

namespace MovieTrackR.Application.Validators.ReviewComments;

public sealed class GetCommentsForReviewQueryValidator : AbstractValidator<GetCommentsForReviewQuery>
{
    public GetCommentsForReviewQueryValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("reviewId");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithName("page");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithName("pageSize");
    }
}