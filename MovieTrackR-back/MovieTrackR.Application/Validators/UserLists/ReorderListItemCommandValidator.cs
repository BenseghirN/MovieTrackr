using FluentValidation;
using MovieTrackR.Application.UserLists.Commands;

namespace MovieTrackR.Application.Validators.UserLists;

public sealed class ReorderListItemCommandValidator : AbstractValidator<ReorderListItemCommand>
{
    public ReorderListItemCommandValidator()
    {
        RuleFor(x => x.currentUser).NotNull();
        RuleFor(x => x.currentUser.ExternalId).NotEmpty();

        RuleFor(x => x.listId).NotEmpty();
        RuleFor(x => x.movieId).NotEmpty();

        RuleFor(x => x.newPosition)
            .GreaterThan(0)
            .WithMessage("NewPosition doit être strictement positif.");
    }
}