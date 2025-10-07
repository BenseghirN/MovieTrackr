using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.ReviewComments;

public sealed class CommentUpdateDtoValidator : AbstractValidator<CommentUpdateDto>
{
    public CommentUpdateDtoValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}