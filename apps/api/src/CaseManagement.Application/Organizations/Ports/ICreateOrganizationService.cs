using CaseManagement.Application.Organizations;

namespace CaseManagement.Application.Ports;

public interface ICreateOrganizationService
{
    Task<OrganizationResult> CreateOrganizationAndSetOwner(
        Guid userId,
        string? name,
        CancellationToken cancellationToken = default);
}