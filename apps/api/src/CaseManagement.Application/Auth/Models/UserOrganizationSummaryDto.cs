namespace CaseManagement.Application.Auth;

public sealed record UserOrganizationSummaryDto(
    Guid Id,
    string Name,
    string Role,
    DateTimeOffset CreatedAtUtc);