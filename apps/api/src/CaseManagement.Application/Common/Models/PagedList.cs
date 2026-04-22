namespace CaseManagement.Application.Common;

public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Skip,
    int Limit,
    bool HasMore);