namespace MovieTrackR.Application.DTOs;

// DTO pour les résultats de recherche
public sealed class SearchPersonResultDto
{
    public Guid? Id { get; set; } // Local ID (nullable car peut venir de TMDB uniquement)
    public int? TmdbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePath { get; set; }
    public string? KnownForDepartment { get; set; } // Acting, Directing, etc.
    public bool IsLocal { get; set; } // true si vient de la DB locale
}

public sealed class PersonDetailsDto
{
    public Guid Id { get; set; }
    public int? TmdbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePath { get; set; }
    public string? Biography { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? KnownForDepartment { get; set; }
    public List<PersonMovieCreditDto> MovieCredits { get; set; } = [];
}
public sealed class PersonMovieCreditDto
{
    public Guid MovieId { get; set; }
    public int? TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public int? Year { get; set; }
    public string? Character { get; set; } // Si acteur
    public string? Job { get; set; } // Si crew (Director, Writer, etc.)
    public string CreditType { get; set; } = "cast"; // "cast" ou "crew"
}

public sealed record CreatePersonDto
{
    public int? TmdbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePath { get; set; }
    public string? Biography { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? PlaceOfBirth { get; set; }
}

public sealed record UpdatePersonDto
{
    public int? TmdbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePath { get; set; }
    public string? Biography { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? PlaceOfBirth { get; set; }
}