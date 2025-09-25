using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.UserLists;

public sealed class ReorderListItemDtoValidator : AbstractValidator<ReorderListItemDto>
{
    public ReorderListItemDtoValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty();
        RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
    }
}