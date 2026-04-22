namespace CaseManagement.Application.Cases;

public sealed record CaseLifecycleCommandInput(
    Guid UserId,
    Guid CaseId,
    string? ClaimedOrganizationId);
