namespace MovieTrackR.Domain.Entities;

public class ReviewComment
{
    public Guid Id { get; private set; }
    public Guid ReviewId { get; private set; }
    public Guid UserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool PubliclyVisible { get; set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Review Review { get; private set; } = null!;
    public User User { get; private set; } = null!;

    public static ReviewComment Create(Guid reviewId, Guid userId, string content)
    {
        return new ReviewComment
        {
            ReviewId = reviewId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Edit(string newContent)
    {
        Content = newContent;
    }

    public void SetVisibility(Boolean visible) => PubliclyVisible = visible;

}