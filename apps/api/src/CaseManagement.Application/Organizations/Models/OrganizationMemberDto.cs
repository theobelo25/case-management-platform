namespace CaseManagement.Application.Organizations;

public sealed record OrganizationMemberDto(
    Guid UserId,
    string Name,
    string Role);