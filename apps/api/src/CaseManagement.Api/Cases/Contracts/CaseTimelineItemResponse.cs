namespace CaseManagement.Api.Cases.Contracts;

public sealed record CaseTimelineItemResponse(
    string Type,
    Guid Id,
    DateTimeOffset CreatedAtUtc,
    Guid? AuthorUserId,
    string? Body,
    bool? IsInternal,
    bool? IsInitial,
    string? EventType,
    string? Metadata);