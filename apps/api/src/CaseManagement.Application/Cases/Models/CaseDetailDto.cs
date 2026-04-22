namespace CaseManagement.Application.Cases;

public sealed record CaseDetailDto(
    Guid Id,
    Guid OrganizationId,
    string Title,
    string Status,
    string Priority,
    string SlaState,
    bool IsArchived,
    DateTimeOffset? SlaDueAtUtc,
    DateTimeOffset? SlaBreachedAtUtc,
    DateTimeOffset? SlaPausedAtUtc,
    int? SlaRemainingSeconds,
    Guid? RequesterUserId,
    string? RequesterName,
    Guid? AssigneeUserId,
    string? AssigneeName,
    Guid CreatedByUserId,
    string? CreatedByName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<CaseTimelineItemDto> Timeline);