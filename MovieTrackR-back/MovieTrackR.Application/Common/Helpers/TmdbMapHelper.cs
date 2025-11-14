namespace MovieTrackR.Application.Common.Helpers;

static class TmdbMapHelpers
{
    public static int? YearFromReleaseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length < 4) return null;
        return int.TryParse(s.AsSpan(0, 4), out var y) ? y : (int?)null;
    }
}