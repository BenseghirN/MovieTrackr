using AutoMapper;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

public sealed record CreateMovieProposalDto(
    string Title,
    string? OriginalTitle,
    int? ReleaseYear,
    string? Country,
    string? Overview,
    string? PosterUrl
);

public sealed record MovieProposalListItemDto(
    Guid Id,
    string Title,
    int? ReleaseYear,
    string? Country,
    MovieProposalStatus Status
);

public sealed record MovieProposalDetailsDto(
    Guid Id,
    string Title,
    string? OriginalTitle,
    int? ReleaseYear,
    string? Country,
    string? Overview,
    string? PosterUrl,
    Guid ProposedByUserId,
    MovieProposalStatus Status,
    string? ModerationNote,
    DateTime? ReviewedAt
);