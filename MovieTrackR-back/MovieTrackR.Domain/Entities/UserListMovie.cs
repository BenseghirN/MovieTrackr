namespace MovieTrackR.Domain.Entities;

public class UserListMovie
{
    public Guid UserListId { get; set; }
    public Guid MovieId { get; set; }
    public int Position { get; set; }

    public UserList UserList { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
