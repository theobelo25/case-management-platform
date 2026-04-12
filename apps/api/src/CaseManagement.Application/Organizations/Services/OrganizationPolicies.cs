using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed class OrganizationPolicies(IOrganizationsRepository organizations) : IOrganizationPolicies
{
    public async Task EnsureUserCanDelete(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var role = await organizations.CheckUserMembership(
                userId,
                organizationId,
                cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        if (role != OrganizationRole.Owner)
            throw new ForbiddenException("Not authorized to delete organization.");
    }

    public async Task EnsureUserCanArchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var role = await organizations.CheckUserMembership(
                userId,
                organizationId,
                cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        if (role != OrganizationRole.Owner && role != OrganizationRole.Admin)
            throw new ForbiddenException("Not authorized to archive organization.");
    }

    public async Task EnsureUserCanUnarchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var role = await organizations.CheckUserMembership(
                userId,
                organizationId,
                cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        if (role != OrganizationRole.Owner && role != OrganizationRole.Admin)
            throw new ForbiddenException("Not authorized to unarchive organization.");
    }
}
