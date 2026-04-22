namespace CaseManagement.Application.Organizations.Ports;

/// <summary>
/// Organization authorization for case lifecycle (Owner or Admin).
/// </summary>
public interface IOrganizationCaseManagementPolicy
{
    Task EnsureUserCanManageCases(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
