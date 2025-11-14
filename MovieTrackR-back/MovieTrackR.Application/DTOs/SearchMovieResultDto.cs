namespace MovieTrackR.Application.DTOs;

public sealed class SearchMovieResultDto
{
    public Guid? LocalId { get; set; }
    public int? TmdbId { get; set; }
    public string Title { get; set; } = default!;
    public int? Year { get; set; }
    public string? OriginalTitle { get; set; }
    public string? PosterPath { get; set; }
    public bool IsLocal { get; set; }
    public double? VoteAverage { get; set; }
    public double? Popularity { get; set; }
    public string? Overview { get; set; }
}