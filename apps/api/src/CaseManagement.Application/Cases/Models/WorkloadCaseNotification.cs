namespace CaseManagement.Application.Cases.Models;

/// <summary>
/// Describes a persisted status and/or priority change for routing in-app notifications.
/// </summary>
public sealed record WorkloadCaseNotification(
    CaseRef Case,
    Guid ActorUserId,
    string? ActorDisplayName,
    bool StatusChanged,
    bool PriorityChanged,
    bool PriorityIncreased,
    string? PreviousStatus,
    string CurrentStatus,
    string? PreviousPriority,
    string CurrentPriority,
    Guid? AssigneeUserId,
    Guid? RequesterUserId);
