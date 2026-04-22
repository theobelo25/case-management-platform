using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationManagementRepository
{
    Task<Organization> Create(
        string name,
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

    Task<Organization> UpdateSlaPolicy(
        Guid organizationId,
        int lowHours,
        int mediumHours,
        int highHours,
        CancellationToken cancellationToken = default);
}
