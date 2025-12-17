using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies;

public sealed class DiscoverCriteria
{
    public int? Year { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public List<int> GenreIds { get; set; } = new List<int>();
    public string Language { get; set; } = "fr-FR";

    public (int Skip, int Take) Paging(int maxPageSize = 100)
    {
        int size = Math.Clamp(PageSize, 1, maxPageSize);
        int page = Math.Max(Page, 1);
        return ((page - 1) * size, size);
    }
}