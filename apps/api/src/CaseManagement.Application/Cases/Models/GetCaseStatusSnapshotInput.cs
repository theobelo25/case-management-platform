namespace CaseManagement.Application.Cases.Models;

public sealed record GetCaseStatusSnapshotInput(
    Guid UserId,
    string? ClaimedOrganizationId);
