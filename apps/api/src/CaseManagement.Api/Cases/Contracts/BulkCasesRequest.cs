namespace CaseManagement.Api.Cases.Contracts;

public sealed record BulkCasesRequest(
    IReadOnlyList<Guid> CaseIds,
    string Action,
    Guid? AssigneeUserId,
    string? Priority,
    string? Status);

public sealed record BulkCasesResultResponse(int UpdatedCount);
