using FluentValidation;
using MovieTrackR.Application.ReviewComments.Commands;

namespace MovieTrackR.Application.Validators.ReviewComments;

public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty().WithName("reviewId");
        RuleFor(x => x.CommentId).NotEmpty().WithName("commentId");
    }
}