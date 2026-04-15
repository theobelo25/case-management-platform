namespace CaseManagement.Api.Common.Contracts;

public sealed record CursorPageResponse<T>(
    IReadOnlyList<T> Items,
    string NextCursor,
    string? PreviousCursor,
    int Limit);
