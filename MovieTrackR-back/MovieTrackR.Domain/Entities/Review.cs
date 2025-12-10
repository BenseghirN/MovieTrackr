namespace MovieTrackR.Domain.Entities;

public class Review
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public float Rating { get; private set; }
    public string? Content { get; private set; }
    public bool PubliclyVisible { get; set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;
    public ICollection<ReviewLike> Likes { get; private set; } = new List<ReviewLike>();
    public ICollection<ReviewComment> Comments { get; private set; } = new List<ReviewComment>();

    public static Review Create(Guid userId, Guid movieId, float rating, string? content)
    {
        return new Review
        {
            UserId = userId,
            MovieId = movieId,
            Rating = rating,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(float rating, string? content)
    {
        Rating = rating;
        Content = content;
    }

    public void AddComment(ReviewComment comment)
    {
        if (!Comments.Any(c => c.Id == comment.Id))
            Comments.Add(comment);
    }

    public int GetLikeCount() => Likes.Count;

    public void RemoveLike(Guid userId)
    {
        ReviewLike? like = Likes.FirstOrDefault(l => l.UserId == userId);
        if (like != null)
        {
            Likes.Remove(like);
        }
    }

    public void AddLike(ReviewLike like)
    {
        if (!Likes.Any(l => l.UserId == like.UserId))
            Likes.Add(like);
    }

    public void SetVisibility(Boolean visible) => PubliclyVisible = visible;
}