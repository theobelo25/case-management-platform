namespace CaseManagement.Application.Organizations.Ports;

/// <summary>
/// Returns user ids with Owner or Admin role in an organization.
/// </summary>
public interface IOrganizationPrivilegedUserIdsQuery
{
    Task<IReadOnlyList<Guid>> GetOwnerAndAdminUserIdsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
