using FluentValidation;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.Application.Validators.Movies;

public sealed class MovieSearchRequestValidator : AbstractValidator<MovieSearchRequest>
{
    public MovieSearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Le numéro de page doit être supérieur ou égal à 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100.");

        RuleFor(x => x.Query)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Query))
            .WithMessage("La recherche ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1800, 2100)
            .When(x => x.Year.HasValue)
            .WithMessage("L'année doit être entre 1800 et 2100.");
    }
}
