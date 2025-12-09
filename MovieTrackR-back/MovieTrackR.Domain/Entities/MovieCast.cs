namespace MovieTrackR.Domain.Entities;

public class MovieCast
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid PersonId { get; set; }
    public string? CharacterName { get; set; }
    public int? Order { get; set; }

    public Movie Movie { get; init; } = null!;
    public Person Person { get; init; } = null!;

    // Constructeur privé pour EF Core
    private MovieCast() { }

    // Factory method interne
    internal static MovieCast Create(Movie movie, Person person, string? characterName, int? order)
    {
        return new MovieCast
        {
            Movie = movie,
            Person = person,
            MovieId = movie.Id,
            PersonId = person.Id,
            CharacterName = characterName,
            Order = order ?? 0
        };
    }

    public void UpdateCharacter(string? name, int? order)
    {
        CharacterName = name;
        Order = order;
    }
}
