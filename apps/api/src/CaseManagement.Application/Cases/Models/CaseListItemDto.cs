namespace CaseManagement.Application.Cases;
public sealed record CaseListItemDto(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    Guid? RequesterUserId,
    string? RequesterName,
    Guid? AssigneeUserId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);