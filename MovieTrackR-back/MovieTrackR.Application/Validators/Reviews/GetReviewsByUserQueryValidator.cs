using FluentValidation;
using MovieTrackR.Application.Reviews.Queries;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class GetReviewsByUserQueryValidator : AbstractValidator<GetReviewsByUserQuery>
{
    public GetReviewsByUserQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithName("userId");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithName("page");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithName("pageSize");
    }
}