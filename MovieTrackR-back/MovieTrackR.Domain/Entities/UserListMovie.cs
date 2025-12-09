namespace MovieTrackR.Domain.Entities;

public class UserListMovie
{
    public Guid UserListId { get; init; }
    public Guid MovieId { get; init; }
    public int Position { get; set; }

    public UserList UserList { get; init; } = null!;
    public Movie Movie { get; init; } = null!;
}
