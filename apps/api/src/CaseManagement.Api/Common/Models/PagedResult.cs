namespace CaseManagement.Api.Common.Contracts;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Skip,
    int Limit,
    bool hasMore);