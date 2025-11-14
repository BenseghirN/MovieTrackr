using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.Application.Validators.Movies;

public sealed class MovieSearchRequestValidator : AbstractValidator<MovieSearchRequest>
{
    public MovieSearchRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Sort).Must(s => s is null or "title" or "year")
            .WithMessage("Sort must be 'title' or 'year'.");
    }
}
