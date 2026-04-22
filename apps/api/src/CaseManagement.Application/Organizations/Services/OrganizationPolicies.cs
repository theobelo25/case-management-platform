using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed class OrganizationPolicies(IOrganizationReadRepository organizations) : IOrganizationPolicies
{
    public async Task EnsureUserCanDelete(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwner(
            userId, 
            organizationId,
            "Must be Owner to delete organization.",
            cancellationToken);
    }

    public async Task EnsureUserCanTransfer(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwner(
            userId,
            organizationId,
            "Must be Owner to transfer ownership.",
            cancellationToken);
    }

    public async Task EnsureUserCanArchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwnerOrAdmin(
            userId, 
            organizationId, 
            "Must be Owner or Admin to archive organization.",
            cancellationToken);
    }

    public async Task EnsureUserCanUnarchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwnerOrAdmin(
            userId, 
            organizationId, 
            "Must be Owner or Admin to unarchive organization.",
            cancellationToken);
    }

    public async Task EnsureUserCanAddMember(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwnerOrAdmin(
            userId, 
            organizationId, 
            "Must be Owner or Admin to add user.",
            cancellationToken);
    }

    public async Task EnsureUserCanManageCases(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwnerOrAdmin(
            userId,
            organizationId,
            "Must be Owner or Admin to manage cases.",
            cancellationToken);
    }

    public async Task EnsureUserCanConfigureSlaPolicy(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await IsUserOwnerOrAdmin(
            userId,
            organizationId,
            "Must be Owner or Admin to configure SLA policy.",
            cancellationToken);
    }

    public async Task EnsureRemoveMemberAllowed(
        Guid actorUserId,
        Guid memberIdToRemove,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        // Always: the membership being removed must not be the org Owner (including self-removal).
        await MemberIsNotOwner(
            memberIdToRemove,
            organizationId,
            "Can not remove Owner from organization. Transfer ownership first.",
            cancellationToken);

        if (actorUserId != memberIdToRemove)
        {
            await IsUserOwnerOrAdmin(
                actorUserId,
                organizationId,
                "Must be Owner or Admin to remove user.",
                cancellationToken);
        }
    }

    private async Task IsUserOwner(
        Guid userId,
        Guid organizationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var role = await organizations.CheckUserMembership(
                userId,
                organizationId,
                cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        if (role != OrganizationRole.Owner)
            throw new ForbiddenException(message);
    }

    private async Task IsUserOwnerOrAdmin(
        Guid userId,
        Guid organizationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var role = await organizations.CheckUserMembership(
                userId,
                organizationId,
                cancellationToken)
            ?? throw new NotFoundException("User is not a member of this organization.");

        if (role != OrganizationRole.Owner && role != OrganizationRole.Admin)
            throw new ForbiddenException(message);
    }

    private async Task MemberIsNotOwner(
        Guid memberId,
        Guid organizationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var role = await organizations.CheckUserMembership(
            memberId,
            organizationId,
            cancellationToken)
            ?? throw new NotFoundException("User is not a member of this organization.");
        
        if (role == OrganizationRole.Owner)
            throw new BadRequestArgumentException(message);
    }
}
