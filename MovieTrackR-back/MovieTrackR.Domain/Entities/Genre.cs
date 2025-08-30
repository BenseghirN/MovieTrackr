namespace MovieTrackR.Domain.Entities;

public class Genre
{
    public Guid Id { get; private set; }
    public int? TmdbId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<MovieGenre> MovieGenres { get; private set; } = new List<MovieGenre>();

    public static Genre Create(string name)
    {
        return new Genre
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void Rename(string newName)
    {
        Name = newName;
    }

    public void AddMovie(Movie movie)
    {
        if (!MovieGenres.Any(mg => mg.MovieId == movie.Id))
        {
            MovieGenres.Add(new MovieGenre
            {
                MovieId = movie.Id,
                GenreId = Id
            });
        }
    }

    public void RemoveMovie(Guid movieId)
    {
        MovieGenre? link = MovieGenres.FirstOrDefault(mg => mg.MovieId == movieId);
        if (link != null)
            MovieGenres.Remove(link);
    }
}