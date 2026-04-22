using CaseManagement.Application.Organizations;

namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationsService
{
    Task<OrganizationResult> AddMember(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
    
    Task RemoveMember(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task TransferOwnership(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

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

    Task<OrganizationSlaPolicyDto> UpdateSlaPolicy(
        Guid userId,
        Guid organizationId,
        int lowHours,
        int mediumHours,
        int highHours,
        CancellationToken cancellationToken = default);
}