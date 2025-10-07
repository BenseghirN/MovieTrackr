using FluentValidation;
using MovieTrackR.Application.ReviewComments.Commands;

namespace MovieTrackR.Application.Validators.ReviewComments;

public sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("reviewId");
        RuleFor(x => x.CommentId).NotEmpty().WithName("commentId");
        RuleFor(x => x.Dto.Content).NotEmpty().MaximumLength(2000).WithName("content");
    }
}