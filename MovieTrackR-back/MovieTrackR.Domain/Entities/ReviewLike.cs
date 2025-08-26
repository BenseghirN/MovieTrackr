namespace MovieTrackR.Domain.Entities;

public class ReviewLike
{
    public Guid UserId { get; private set; }
    public Guid ReviewId { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;
    public Review Review { get; private set; } = null!;

    public static ReviewLike Create(Guid userId, Guid reviewId)
    {
        return new ReviewLike
        {
            UserId = userId,
            ReviewId = reviewId
        };
    }
}