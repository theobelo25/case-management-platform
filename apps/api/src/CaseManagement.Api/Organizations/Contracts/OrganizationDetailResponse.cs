namespace CaseManagement.Api.Organizations.Contracts;
public sealed record OrganizationDetailResponse(
    OrganizationResponse Organization,
    IReadOnlyList<OrganizationMemberResponse> Members);