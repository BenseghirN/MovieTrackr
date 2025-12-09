namespace MovieTrackR.Application.Movies;

public sealed class MovieSearchCriteria
{
    public string? Query { get; init; }
    public int? Year { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public (int Skip, int Take) Paging(int maxPageSize = 100)
    {
        int size = Math.Clamp(PageSize, 1, maxPageSize);
        int page = Math.Max(Page, 1);
        return ((page - 1) * size, size);
    }
}