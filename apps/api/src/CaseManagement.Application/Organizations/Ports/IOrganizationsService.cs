namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationsService
{
    Task<OrganizationResult> Archive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<OrganizationResult> Unarchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task Delete(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}