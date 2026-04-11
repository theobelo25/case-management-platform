namespace CaseManagement.Application.Organizations;

public sealed record OrganizationDetailDto(
    Guid OrganizationId,
    string OrganizationName,
    DateTimeOffset OrganizationCreatedAtUtc,
    IReadOnlyList<OrganizationMemberDto> Members);