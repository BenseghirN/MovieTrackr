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
    public Guid ProposedByUserId { get; set; }
    public MovieProposalStatus Status { get; set; } = MovieProposalStatus.UnderReview;
    public string? ModerationNote { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public User ProposedByUser { get; set; } = default!;
}
