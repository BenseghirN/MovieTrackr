using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Domain.Entities;

public class MovieProposal
{
    public Guid Id { get; init; }
    public string Title { get; set; } = default!;
    public string? OriginalTitle { get; set; }
    public int? ReleaseYear { get; set; }
    public string? Country { get; set; }
    public string? Overview { get; set; }
    public string? PosterUrl { get; set; }
    public Guid ProposedByUserId { get; init; }
    public MovieProposalStatus Status { get; set; } = MovieProposalStatus.UnderReview;
    public string? ModerationNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public User ProposedByUser { get; init; } = default!;

    public static MovieProposal Create(
        Guid proposedByUserId,
        string title,
        string? originalTitle = null,
        int? releaseYear = null,
        string? country = null,
        string? overview = null,
        string? posterUrl = null)
    {
        return new MovieProposal
        {
            ProposedByUserId = proposedByUserId,
            Title = title,
            OriginalTitle = originalTitle,
            ReleaseYear = releaseYear,
            Country = country,
            Overview = overview,
            PosterUrl = posterUrl,
            Status = MovieProposalStatus.UnderReview,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve(string? note = null)
    {
        Status = MovieProposalStatus.Approved;
        ModerationNote = note;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(string? note = null)
    {
        Status = MovieProposalStatus.Rejected;
        ModerationNote = note;
        ReviewedAt = DateTime.UtcNow;
    }
}
