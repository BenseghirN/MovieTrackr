namespace MovieTrackR.Domain.Entities;

public class MovieCrew
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid PersonId { get; set; }
    public string Job { get; set; } = string.Empty;
    public string? Department { get; set; }

    public Movie Movie { get; private set; } = null!;
    public Person Person { get; private set; } = null!;

    public void UpdateRole(string job, string? department)
    {
        Job = job;
        Department = department;
    }
}
