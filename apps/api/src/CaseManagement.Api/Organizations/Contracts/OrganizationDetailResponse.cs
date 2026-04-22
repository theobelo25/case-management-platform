namespace CaseManagement.Api.Organizations.Contracts;
public sealed record OrganizationDetailResponse(
    OrganizationResponse Organization,
    OrganizationSlaPolicyResponse SlaPolicy,
    IReadOnlyList<OrganizationMemberResponse> Members);