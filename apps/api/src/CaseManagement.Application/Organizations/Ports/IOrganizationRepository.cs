
using CaseManagement.Application.Organizations;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface IOrganizationRepository
{
    Task<OrganizationResult> Create(
        string name, 
        CancellationToken cancellationToken = default);

    Task<OrganizationMembershipResult> IssueMembership(
        Guid userId,
        Guid organizationId,
        OrganizationRole role,
        CancellationToken cancellationToken = default);
}