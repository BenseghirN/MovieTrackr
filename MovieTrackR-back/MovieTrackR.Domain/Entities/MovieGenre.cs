namespace MovieTrackR.Domain.Entities;

public class MovieGenre
{
    public Guid MovieId { get; init; }
    public Guid GenreId { get; init; }

    public Movie Movie { get; init; } = null!;
    public Genre Genre { get; init; } = null!;
}
