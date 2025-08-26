namespace MovieTrackR.Domain.Entities;

public class MovieGenre
{
    public Guid MovieId { get; set; }
    public Guid GenreId { get; set; }

    public Movie Movie { get; private set; } = null!;
    public Genre Genre { get; private set; } = null!;
}
