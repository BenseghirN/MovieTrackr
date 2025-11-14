namespace MovieTrackR.Domain.Entities;

public class MovieCast
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid PersonId { get; set; }
    public string? CharacterName { get; set; }
    public int? Order { get; set; }

    public Movie Movie { get; private set; } = null!;
    public Person Person { get; private set; } = null!;

    public void UpdateCharacter(string? name, int? order)
    {
        CharacterName = name;
        Order = order;
    }
}
