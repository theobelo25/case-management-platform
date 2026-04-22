namespace CaseManagement.Application.Cases.Models;

public sealed record GetFirstResponseTimeOverTimeInput(
    Guid UserId,
    string? ClaimedOrganizationId,
    int Days);
