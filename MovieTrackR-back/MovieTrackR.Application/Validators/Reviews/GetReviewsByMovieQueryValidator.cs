using FluentValidation;
using MovieTrackR.Application.Reviews.Queries;

namespace MovieTrackR.Application.Validators.Reviews;

public sealed class GetReviewsByMovieQueryValidator : AbstractValidator<GetReviewsByMovieQuery>
{
    public GetReviewsByMovieQueryValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty().WithName("movieId");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithName("page");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithName("pageSize");
    }
}