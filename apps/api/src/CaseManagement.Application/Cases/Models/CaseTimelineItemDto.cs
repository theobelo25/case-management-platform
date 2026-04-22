namespace CaseManagement.Application.Cases;

public sealed record CaseTimelineItemDto(
    string Type,
    Guid Id,
    DateTimeOffset CreatedAtUtc,
    Guid? AuthorUserId,
    string? AuthorDisplayName,
    string? Body,
    bool? IsInternal,
    bool? IsInitial,
    string? EventType,
    string? Metadata);