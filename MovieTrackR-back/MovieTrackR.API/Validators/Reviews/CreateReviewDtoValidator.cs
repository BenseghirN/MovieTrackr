using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.Reviews;

public sealed class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(0, 5);
        RuleFor(x => x.Content).MaximumLength(2000);
    }
}