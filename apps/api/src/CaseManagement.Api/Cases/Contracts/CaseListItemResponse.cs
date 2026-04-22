namespace CaseManagement.Api.Cases.Contracts;
public sealed record CaseListItemResponse(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    string SlaState,
    DateTimeOffset? SlaDueAtUtc,
    DateTimeOffset? SlaBreachedAtUtc,
    DateTimeOffset? SlaPausedAtUtc,
    int? SlaRemainingSeconds,
    Guid? RequesterUserId,
    string? RequesterName,
    Guid? AssigneeUserId,
    string? AssigneeName,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);