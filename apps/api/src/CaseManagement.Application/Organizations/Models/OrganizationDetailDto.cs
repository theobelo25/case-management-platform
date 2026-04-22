namespace CaseManagement.Application.Organizations;

public sealed record OrganizationDetailDto(
    Guid OrganizationId,
    string OrganizationName,
    DateTimeOffset OrganizationCreatedAtUtc,
    bool OrganizationIsArchived,
    int SlaLowHours,
    int SlaMediumHours,
    int SlaHighHours,
    IReadOnlyList<OrganizationMemberDto> Members);