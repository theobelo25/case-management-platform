namespace CaseManagement.Application.Organizations;

public sealed record OrganizationResult(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAtUtc,
    bool IsArchived
);
