namespace MovieTrackR.Domain.Entities;

public class Movie
{
    public Guid Id { get; private set; }
    public int? TmdbId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? OriginalTitle { get; private set; }
    public int? Year { get; private set; }
    public string? PosterUrl { get; private set; }
    public string? BackdropPath { get; private set; }
    public string? TrailerUrl { get; private set; }
    public int? Duration { get; private set; }
    public string? Overview { get; private set; }
    public DateTime? ReleaseDate { get; private set; }
    public double? VoteAverage { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<MovieGenre> MovieGenres { get; private set; } = new List<MovieGenre>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<MovieCast> Cast { get; private set; } = new List<MovieCast>();
    public ICollection<MovieCrew> Crew { get; private set; } = new List<MovieCrew>();

    // Factory method
    public static Movie CreateNew(
        string title,
        int? tmdbId,
        string? originalTitle,
        int? year,
        string? posterUrl,
        string? backdropPath,
        string? trailerUrl,
        int? duration,
        string? overview,
        DateTime? releaseDate,
        double? voteAverage)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            TmdbId = tmdbId,
            Title = title,
            OriginalTitle = originalTitle,
            Year = year,
            PosterUrl = posterUrl,
            BackdropPath = backdropPath,
            TrailerUrl = trailerUrl,
            Duration = duration,
            Overview = overview,
            ReleaseDate = releaseDate,
            VoteAverage = voteAverage,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string title,
        string? originalTitle,
        int? year,
        string? posterUrl,
        string? backdropPath,
        string? trailerUrl,
        int? duration,
        string? overview,
        double? voteAverage,
        DateTime? releaseDate)
    {
        Title = title;
        OriginalTitle = originalTitle;
        Year = year;
        PosterUrl = posterUrl;
        BackdropPath = backdropPath;
        TrailerUrl = trailerUrl;
        Duration = duration;
        Overview = overview;
        ReleaseDate = releaseDate;
        VoteAverage = voteAverage;
    }

    public void AddGenre(Genre genre)
    {
        if (!MovieGenres.Any(mg => mg.GenreId == genre.Id))
        {
            MovieGenres.Add(new MovieGenre
            {
                MovieId = Id,
                GenreId = genre.Id
            });
        }
    }

    public void RemoveGenre(Guid genreId)
    {
        MovieGenre? genreToRemove = MovieGenres.FirstOrDefault(mg => mg.GenreId == genreId);
        if (genreToRemove != null)
        {
            MovieGenres.Remove(genreToRemove);
        }
    }

    public void AddCast(Person person, string? characterName, int? order)
    {
        if (!Cast.Any(c => c.PersonId == person.Id))
        {
            Cast.Add(new MovieCast
            {
                Id = Guid.NewGuid(),
                MovieId = Id,
                PersonId = person.Id,
                CharacterName = characterName,
                Order = order
            });
        }
    }

    public void RemoveCast(Guid personId)
    {
        MovieCast? castToRemove = Cast.FirstOrDefault(c => c.PersonId == personId);
        if (castToRemove != null)
        {
            Cast.Remove(castToRemove);
        }
    }

    public void AddCrew(Person person, string job, string? department)
    {
        if (!Crew.Any(c => c.PersonId == person.Id && c.Job == job))
        {
            Crew.Add(new MovieCrew
            {
                Id = Guid.NewGuid(),
                MovieId = Id,
                PersonId = person.Id,
                Job = job,
                Department = department
            });
        }
    }

    public void RemoveCrew(Guid personId, string job)
    {
        MovieCrew? crewToRemove = Crew.FirstOrDefault(c => c.PersonId == personId && c.Job == job);
        if (crewToRemove != null)
        {
            Crew.Remove(crewToRemove);
        }
    }
}
