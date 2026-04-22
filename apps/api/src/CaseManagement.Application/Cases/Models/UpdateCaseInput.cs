namespace CaseManagement.Application.Cases;

public sealed record UpdateCaseInput(
    Guid UserId,
    Guid CaseId,
    string? ClaimedOrganizationId,
    CaseStatusCode Status,
    CasePriorityCode Priority);
