namespace CaseManagement.Api.Cases.Contracts;

public sealed record CaseDetailResponse(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    Guid? RequesterUserId,
    string? RequesterName,
    Guid? AssigneeUserId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<CaseTimelineItemResponse> Timeline);