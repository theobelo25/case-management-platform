namespace CaseManagement.Application.Cases.Models;

/// <summary>
/// Captures assignee transition for a single case so realtime notifications can be sent after persistence.
/// </summary>
public sealed record CaseAssignmentChange(
    Guid OrganizationId,
    Guid CaseId,
    string CaseTitle,
    Guid? FromAssigneeUserId,
    Guid? ToAssigneeUserId,
    string? FromDisplayName,
    string? ToDisplayName);
