using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.Reviews;

public sealed class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewDtoValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(0f, 10f);
        RuleFor(x => x.Content).MaximumLength(4000);
    }
}