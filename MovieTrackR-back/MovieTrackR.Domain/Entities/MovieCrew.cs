namespace MovieTrackR.Domain.Entities;

public class MovieCrew
{
    public Guid Id { get; init; }
    public Guid MovieId { get; init; }
    public Guid PersonId { get; init; }
    public string Job { get; set; } = string.Empty;
    public string? Department { get; set; }

    public Movie Movie { get; init; } = null!;
    public Person Person { get; init; } = null!;

    private MovieCrew() { }

    internal static MovieCrew Create(Movie movie, Person person, string job, string? department)
    {
        return new MovieCrew
        {
            Movie = movie,
            Person = person,
            MovieId = movie.Id,
            PersonId = person.Id,
            Job = job,
            Department = department
        };
    }

    public void UpdateRole(string job, string? department)
    {
        Job = job;
        Department = department;
    }
}
