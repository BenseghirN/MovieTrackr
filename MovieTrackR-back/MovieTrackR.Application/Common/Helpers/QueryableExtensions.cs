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

        bool isNpgsql = dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;
        string needle = search.Trim();

        if (isNpgsql)
        {
            string pattern = $"%{needle.Replace("%", "\\%").Replace("_", "\\_")}%";
            return query.Where(m =>
                EF.Functions.ILike(m.Title!, pattern) ||
                (m.OriginalTitle != null && EF.Functions.ILike(m.OriginalTitle!, pattern)));
        }

        string lower = needle.ToLowerInvariant();
        return query.Where(m =>
            (m.Title != null && m.Title.ToLower().Contains(lower)) ||
            (m.OriginalTitle != null && m.OriginalTitle.ToLower().Contains(lower)));
    }
}