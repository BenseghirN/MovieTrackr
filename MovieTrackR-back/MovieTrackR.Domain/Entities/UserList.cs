namespace MovieTrackR.Domain.Entities;

public class UserList
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;
    public ICollection<UserListMovie> Movies { get; private set; } = new List<UserListMovie>();

    public static UserList Create(Guid userId, string title, string? description)
    {
        return new UserList
        {
            UserId = userId,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description)
    {
        Title = title;
        Description = description;
    }

    public void AddMovie(Movie movie, int position)
    {
        if (!Movies.Any(m => m.MovieId == movie.Id))
        {
            if (Movies.Any(m => m.Position == position))
                throw new InvalidOperationException("Position already used.");

            Movies.Add(new UserListMovie
            {
                UserListId = Id,
                MovieId = movie.Id,
                Position = position
            });
        }
    }

    public void RemoveMovie(Guid movieId)
    {
        UserListMovie? item = Movies.FirstOrDefault(m => m.MovieId == movieId);
        if (item != null)
            Movies.Remove(item);
    }

    public void ReorderMovie(Guid movieId, int newPosition)
    {
        UserListMovie? item = Movies.FirstOrDefault(m => m.MovieId == movieId);
        if (item != null)
            item.Position = newPosition;
    }
}
