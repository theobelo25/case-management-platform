using CaseManagement.Application.Cases;

namespace CaseManagement.Application.Cases.Models;

public enum BulkCaseAction
{
    Assign,
    SetPriority,
    SetStatus,
    BumpPriority
}

public sealed record BulkCasesInput(
    Guid UserId,
    string? ClaimedOrganizationId,
    IReadOnlyList<Guid> CaseIds,
    BulkCaseAction Action,
    Guid? AssigneeUserId,
    CasePriorityCode? Priority,
    CaseStatusCode? Status);

public sealed record BulkCasesResultDto(int UpdatedCount);
