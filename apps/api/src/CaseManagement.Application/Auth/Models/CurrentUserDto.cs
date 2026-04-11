namespace CaseManagement.Application.Auth;

public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid ActiveOrganizationId,
    IReadOnlyList<UserOrganizationSummaryDto> Organizations);