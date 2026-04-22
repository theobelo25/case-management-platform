using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationReadRepository
{
    Task<Organization?> GetById(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<OrganizationRole?> CheckUserMembership(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// User ids with Owner or Admin role in the organization (for audit notifications).
    /// </summary>
    Task<IReadOnlyList<Guid>> GetOwnerAndAdminUserIds(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
