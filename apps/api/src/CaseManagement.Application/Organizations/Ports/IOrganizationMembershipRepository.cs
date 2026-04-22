using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationMembershipRepository
{
    Task<OrganizationMembership> IssueMembership(
        Guid userId,
        Guid organizationId,
        OrganizationRole role,
        CancellationToken cancellationToken = default);

    Task<int> RevokeMembership(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task TransferOwnership(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
