
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface IOrganizationsRepository
{
    Task<Organization?> GetById(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<Organization> Create(
        string name, 
        CancellationToken cancellationToken = default);

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

    Task<OrganizationRole?> CheckUserMembership(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<Organization> Archive(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<Organization> Unarchive(
        Guid organizationId,
        CancellationToken cancellationToken = default);
    
    Task<bool> Delete(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}