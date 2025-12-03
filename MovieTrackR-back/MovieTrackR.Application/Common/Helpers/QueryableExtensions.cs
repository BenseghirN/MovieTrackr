using Microsoft.EntityFrameworkCore;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Common.Helpers;

public static class QueryableExtensions
{
    public static IQueryable<Movie> ApplyTitleFilter(
        this IQueryable<Movie> query,
        DbContext dbContext,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        bool isNpgsql = dbContext.Database.ProviderName?
            .Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

        string needle = search.Trim();
        string withSpaces = needle.Replace("-", " ").Trim();
        withSpaces = System.Text.RegularExpressions.Regex.Replace(withSpaces, @"\s+", " ");

        string withHyphens = needle.Replace(" ", "-").Trim();
        string noSeparator = needle.Replace(" ", "").Replace("-", "").Trim();

        if (isNpgsql)
        {
            static string EscapeLike(string s) =>
                s.Replace("\\", "\\\\")
                 .Replace("%", "\\%")
                 .Replace("_", "\\_");

            string patternSpace = $"%{EscapeLike(withSpaces)}%";
            string patternHyphen = $"%{EscapeLike(withHyphens)}%";
            string patternNoSep = $"%{EscapeLike(noSeparator)}%";

            return query.Where(m =>
                (m.Title != null && (
                    EF.Functions.ILike(m.Title, patternSpace) ||
                    EF.Functions.ILike(m.Title, patternHyphen) ||
                    EF.Functions.ILike(m.Title.Replace(" ", "").Replace("-", ""), patternNoSep)
                )) ||
                (m.OriginalTitle != null && (
                    EF.Functions.ILike(m.OriginalTitle!, patternSpace) ||
                    EF.Functions.ILike(m.OriginalTitle!, patternHyphen) ||
                    EF.Functions.ILike(m.OriginalTitle.Replace(" ", "").Replace("-", ""), patternNoSep)
                )));
        }
        else
        {
            string lowerSpace = withSpaces.ToLowerInvariant();
            string lowerHyphen = withHyphens.ToLowerInvariant();
            string lowerNoSep = noSeparator.ToLowerInvariant();

            return query.Where(m =>
                (m.Title != null && (
                    m.Title.ToLower().Contains(lowerSpace) ||
                    m.Title.ToLower().Contains(lowerHyphen) ||
                    m.Title.ToLower().Replace(" ", "").Replace("-", "").Contains(lowerNoSep)
                )) ||
                (m.OriginalTitle != null && (
                    m.OriginalTitle.ToLower().Contains(lowerSpace) ||
                    m.OriginalTitle.ToLower().Contains(lowerHyphen) ||
                    m.OriginalTitle.ToLower().Replace(" ", "").Replace("-", "").Contains(lowerNoSep)
                )));
        }
    }
}