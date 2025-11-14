namespace MovieTrackR.Application.DTOs;


public sealed record PageMeta(
    int Page,
    int PageSize,
    int TotalLocal,
    int TotalTmdb,
    int TotalResults,
    int? TotalTmdbPages,
    bool HasMore
);

public sealed record HybridPagedResult<T>(
    IReadOnlyList<T> Items,
    PageMeta Meta
);
