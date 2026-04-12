using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Ports;

namespace CaseManagement.Application.Organizations;

public sealed class OrganizationsService(
    IOrganizationsRepository organizations,
    IOrganizationPolicies policies) : IOrganizationsService
{
    public async Task<OrganizationResult> Archive(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureUserCanArchive(userId, organizationId, cancellationToken);

        var archivedOrganization = await organizations.Archive(
            organizationId,
            cancellationToken);

        return new OrganizationResult(
            archivedOrganization.Id,
            archivedOrganization.Name,
            archivedOrganization.CreatedAtUtc,
            archivedOrganization.IsArchived);
    }

    public async Task<OrganizationResult> Unarchive(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureUserCanUnarchive(userId, organizationId, cancellationToken);

        var unarchivedOrganization = await organizations.Unarchive(
            organizationId, 
            cancellationToken);

        return new OrganizationResult(
            unarchivedOrganization.Id,
            unarchivedOrganization.Name,
            unarchivedOrganization.CreatedAtUtc,
            unarchivedOrganization.IsArchived);
    }

    public async Task Delete(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureUserCanDelete(
            userId, 
            organizationId, 
            cancellationToken);

        var isDeleted = await organizations.Delete(
            organizationId, 
            cancellationToken);
        if (!isDeleted)
            throw new NotFoundException("Organization not found.");
    }
}