namespace MovieTrackR.Domain.Entities;

public class Person
{
    public Guid Id { get; private set; }
    public int? TmdbId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ProfilePictureUrl { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public DateTime? DeathDate { get; private set; }
    public string? Biography { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<MovieCast> CastRoles { get; private set; } = new List<MovieCast>();
    public ICollection<MovieCrew> CrewRoles { get; private set; } = new List<MovieCrew>();

    public static Person Create(string name, int? tmdbId = null)
    {
        return new Person
        {
            Id = Guid.NewGuid(),
            Name = name,
            TmdbId = tmdbId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string? profileUrl, DateTime? birth, DateTime? death, string? bio)
    {
        Name = name;
        ProfilePictureUrl = profileUrl;
        BirthDate = birth;
        DeathDate = death;
        Biography = bio;
    }
}