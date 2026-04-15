using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed class OrganizationsService(
    IOrganizationsRepository organizations,
    IUnitOfWork unitOfWork,
    IUserRepository users,
    IOrganizationPolicies policies) : IOrganizationsService
{
    public async Task<OrganizationResult> AddMember(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureUserCanAddMember(
            userId, 
            organizationId, 
            cancellationToken);

        if (await users.GetByIdAsync(memberId, cancellationToken) is null)
            throw new NotFoundException("Member not found.");

        var isMember = await organizations.CheckUserMembership(
            memberId,
            organizationId,
            cancellationToken);
        if (isMember is not null)
            throw new ConflictException("User is already a member.");

        await organizations.IssueMembership(
            memberId, 
            organizationId, 
            OrganizationRole.Member, 
            cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var organization = await organizations.GetById(
            organizationId, 
            cancellationToken)
            ?? throw new NotFoundException("Organization not found.");
        
        return new OrganizationResult(
            organization.Id,
            organization.Name,
            organization.CreatedAtUtc,
            organization.IsArchived);
    }

    public async Task RemoveMember(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureRemoveMemberAllowed(
            userId,
            memberId,
            organizationId,
            cancellationToken);

        var revokedUsers = await organizations.RevokeMembership(
            memberId, 
            organizationId, 
            cancellationToken);
        
        if (revokedUsers == 0)
            throw new NotFoundException("User membership not found.");
        
    }

    public async Task TransferOwnership(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (userId == memberId)
            throw new BadRequestArgumentException("Cannot transfer ownership to yourself.");
            
        await policies.EnsureUserCanTransfer(
            userId, 
            organizationId, 
            cancellationToken);

        await organizations.TransferOwnership(
            userId,
            memberId,
            organizationId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationResult> Archive(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureUserCanArchive(
            userId, 
            organizationId, 
            cancellationToken);

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