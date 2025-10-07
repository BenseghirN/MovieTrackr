using FluentValidation;
using MovieTrackR.Application.ReviewComments.Commands;

namespace MovieTrackR.Application.Validators.ReviewComments;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("reviewId");
        RuleFor(x => x.Dto.Content).NotEmpty().MaximumLength(2000).WithName("content");
    }
}