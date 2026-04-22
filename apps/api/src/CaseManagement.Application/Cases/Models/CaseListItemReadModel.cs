using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases;

public sealed record CaseListItemReadModel(
    Guid Id,
    string Title,
    CaseStatus Status,
    CasePriority Priority,
    DateTimeOffset? SlaDueAtUtc,
    DateTimeOffset? SlaBreachedAtUtc,
    DateTimeOffset? SlaPausedAtUtc,
    int? SlaRemainingSeconds,
    Guid? RequesterUserId,
    string? RequesterName,
    Guid? AssigneeUserId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
