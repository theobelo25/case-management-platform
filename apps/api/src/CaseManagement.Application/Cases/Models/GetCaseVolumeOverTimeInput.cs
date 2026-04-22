namespace CaseManagement.Application.Cases.Models;

public sealed record GetCaseVolumeOverTimeInput(
    Guid UserId,
    string? ClaimedOrganizationId,
    int Days);
