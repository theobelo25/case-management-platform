using CaseManagement.Application.Organizations;

namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationDetailQuery
{
    Task<OrganizationDetailDto?> GetDetailForMemberAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}