namespace MovieTrackR.Domain.Entities;

public class ReviewLike
{
    public Guid UserId { get; init; }
    public Guid ReviewId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public User User { get; init; } = null!;
    public Review Review { get; init; } = null!;

    public static ReviewLike Create(Guid userId, Guid reviewId)
    {
        return new ReviewLike
        {
            UserId = userId,
            ReviewId = reviewId,
            CreatedAt = DateTime.UtcNow
        };
    }
}