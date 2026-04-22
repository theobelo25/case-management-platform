namespace CaseManagement.Application.Cases;

public sealed record AssignCaseInput(
    Guid UserId,
    Guid CaseId,
    string? ClaimedOrganizationId,
    Guid? AssigneeUserId);
