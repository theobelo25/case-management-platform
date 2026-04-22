using CaseManagement.Api.Organizations.Contracts;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Organizations;

namespace CaseManagement.Api.Controllers;

internal static class OrganizationResponseMapper
{
    public static UserOrganizationResponse MapUserOrganization(UserOrganizationSummaryDto dto)
    {
        return new UserOrganizationResponse(
            dto.Id,
            dto.Name,
            dto.Role,
            dto.IsArchived);
    }

    public static OrganizationResponse MapOrganization(OrganizationResult dto)
    {
        return new OrganizationResponse(
            dto.Id,
            dto.Name,
            dto.CreatedAtUtc,
            dto.IsArchived);
    }

    public static OrganizationSlaPolicyResponse MapSlaPolicy(OrganizationSlaPolicyDto dto)
    {
        return new OrganizationSlaPolicyResponse(
            dto.LowHours,
            dto.MediumHours,
            dto.HighHours);
    }

    public static OrganizationDetailResponse MapOrganizationDetail(OrganizationDetailDto dto)
    {
        return new OrganizationDetailResponse(
            new OrganizationResponse(
                dto.OrganizationId,
                dto.OrganizationName,
                dto.OrganizationCreatedAtUtc,
                dto.OrganizationIsArchived),
            new OrganizationSlaPolicyResponse(
                dto.SlaLowHours,
                dto.SlaMediumHours,
                dto.SlaHighHours),
            dto.Members.Select(MapOrganizationMember).ToArray());
    }

    private static OrganizationMemberResponse MapOrganizationMember(OrganizationMemberDto dto)
    {
        return new OrganizationMemberResponse(
            dto.UserId,
            dto.Name,
            dto.Role,
            dto.Email,
            dto.JoinedAtUtc);
    }
}
