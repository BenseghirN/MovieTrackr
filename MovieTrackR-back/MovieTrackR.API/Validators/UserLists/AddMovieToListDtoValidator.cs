using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.UserLists;

public sealed class AddMovieToListDtoValidator : AbstractValidator<AddMovieToListDto>
{
    public AddMovieToListDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.MovieId.HasValue ? 1 : 0) + (x.TmdbId.HasValue ? 1 : 0) == 1)
            .WithMessage("Fournir soit movieId, soit tmdbId (exclusivement).");

        RuleFor(x => x.TmdbId)
            .GreaterThan(0)
            .When(x => x.TmdbId.HasValue);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Position.HasValue);
    }
}