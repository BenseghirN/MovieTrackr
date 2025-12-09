using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.Reviews;

public sealed class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(0, 5);
        RuleFor(x => x.Content).MaximumLength(5000).WithMessage("Review content is too long.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Review content cannot be empty.");
    }
}