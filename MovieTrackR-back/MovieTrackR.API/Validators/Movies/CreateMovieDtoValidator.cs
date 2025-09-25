using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Validators.Movies;

public sealed class CreateMovieDtoValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre est requis.")
            .MaximumLength(300);

        RuleFor(x => x.TmdbId)
            .GreaterThan(0)
            .When(x => x.TmdbId.HasValue);

        RuleFor(x => x.Year)
            .InclusiveBetween(1888, 2100)
            .When(x => x.Year.HasValue);

        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 600) // 1 min à 10h
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.ReleaseDate.HasValue);

        RuleFor(x => x.GenreIds)
            .Must(list => list is null || list.Distinct().Count() == list.Count)
            .WithMessage("GenreIds ne doit pas contenir de doublons.");
    }
}