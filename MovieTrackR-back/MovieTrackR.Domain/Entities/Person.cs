namespace MovieTrackR.Domain.Entities;

public class Person
{
    public Guid Id { get; private set; }
    public int? TmdbId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ProfilePictureUrl { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public DateTime? DeathDate { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? Biography { get; private set; }
    public string? KnownForDepartment { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<MovieCast> CastRoles { get; private set; } = new List<MovieCast>();
    public ICollection<MovieCrew> CrewRoles { get; private set; } = new List<MovieCrew>();
    private Person() { }

    public static Person Create(
        string name,
        DateTime? birthDate,
        DateTime? deathDate,
        string? biography,
        string? placeOfBirth,
        int? tmdbId = null,
        string? ProfilePictureUrl = null)
    {
        return new Person
        {
            Name = name,
            TmdbId = tmdbId,
            ProfilePictureUrl = ProfilePictureUrl,
            BirthDate = birthDate,
            DeathDate = deathDate,
            Biography = biography,
            PlaceOfBirth = placeOfBirth,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string name,
        string? profilePath,
        DateTime? birth,
        DateTime? death,
        string? bio,
        string? placeOfBirth,
        string? knownForDepartment)
    {
        Name = name;
        ProfilePictureUrl = profilePath;
        BirthDate = birth;
        DeathDate = death;
        Biography = bio;
        PlaceOfBirth = placeOfBirth;
        KnownForDepartment = knownForDepartment;
    }
}