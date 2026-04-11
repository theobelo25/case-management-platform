
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface IOrganizationRepository
{
    Task<Organization> Create(
        string name, 
        CancellationToken cancellationToken = default);

    Task<OrganizationMembership> IssueMembership(
        Guid userId,
        Guid organizationId,
        OrganizationRole role,
        CancellationToken cancellationToken = default);

    Task<OrganizationRole?> CheckUserMembership(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}