namespace CaseManagement.Application.Organizations;

public sealed record OrganizationDetailDto(
    Guid OrganizationId,
    string OrganizationName,
    DateTimeOffset OrganizationCreatedAtUtc,
    bool OrganizationIsArchived,
    IReadOnlyList<OrganizationMemberDto> Members);