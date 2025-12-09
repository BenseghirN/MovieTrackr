using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Domain.Entities;

public class UserList
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public UserListType Type { get; set; } = UserListType.Custom;
    public bool IsSystemList => Type != UserListType.Custom;

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

    public static UserList CreateSystemList(Guid userId, string title, string? description, UserListType type)
    {
        if (type == UserListType.Custom)
            throw new ArgumentException("Use Create() method for custom lists", nameof(type));

        return new UserList
        {
            UserId = userId,
            Title = title,
            Description = description,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserList CreateWatchlist(Guid userId)
    {
        return CreateSystemList(userId, "À regarder", "Films que je veux regarder", UserListType.Watchlist);
    }

    public static UserList CreateFavorites(Guid userId)
    {
        return CreateSystemList(userId, "Favoris", "Mes films préférés", UserListType.Favorites);
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
