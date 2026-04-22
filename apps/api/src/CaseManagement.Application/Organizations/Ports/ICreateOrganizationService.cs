using CaseManagement.Application.Organizations;

namespace CaseManagement.Application.Organizations.Ports;

public interface ICreateOrganizationService
{
    Task<OrganizationResult> CreateOrganizationAndSetOwner(
        Guid userId,
        string? name,
        CancellationToken cancellationToken = default);
}