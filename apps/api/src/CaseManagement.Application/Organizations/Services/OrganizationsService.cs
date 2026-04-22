using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed class OrganizationsService(
    IOrganizationReadRepository organizationReadRepository,
    IOrganizationMembershipRepository organizationMembershipRepository,
    IOrganizationManagementRepository organizationManagementRepository,
    IUnitOfWork unitOfWork,
    IUserRepository users,
    IUserDisplayNameLookup userDisplayNames,
    IOrganizationPolicies policies,
    IOrganizationMembershipNotifier membershipNotifier) : IOrganizationsService
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

        var isMember = await organizationReadRepository.CheckUserMembership(
            memberId,
            organizationId,
            cancellationToken);
        if (isMember is not null)
            throw new ConflictException("User is already a member.");

        await organizationMembershipRepository.IssueMembership(
            memberId, 
            organizationId, 
            OrganizationRole.Member, 
            cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var organization = await organizationReadRepository.GetById(
            organizationId, 
            cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        var performerNames = await userDisplayNames.GetDisplayNamesByIdsAsync(
            new[] { userId },
            cancellationToken);
        performerNames.TryGetValue(userId, out var performerDisplayName);

        await membershipNotifier.NotifyMemberAddedAsync(
            memberId,
            organization.Id,
            organization.Name,
            userId,
            performerDisplayName,
            cancellationToken);
        
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

        var organization = await organizationReadRepository.GetById(organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        var displayNames = await userDisplayNames.GetDisplayNamesByIdsAsync(
            new[] { userId, memberId },
            cancellationToken);
        displayNames.TryGetValue(memberId, out var removedMemberDisplayName);
        var removedLabel = string.IsNullOrWhiteSpace(removedMemberDisplayName)
            ? "A member"
            : removedMemberDisplayName;
        displayNames.TryGetValue(userId, out var performerDisplayName);

        var ownerAndAdminIds = await organizationReadRepository.GetOwnerAndAdminUserIds(
            organizationId,
            cancellationToken);

        var revokedUsers = await organizationMembershipRepository.RevokeMembership(
            memberId, 
            organizationId, 
            cancellationToken);
        
        if (revokedUsers == 0)
            throw new NotFoundException("User membership not found.");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var auditRecipients = ownerAndAdminIds
            .Where(id => id != userId && id != memberId)
            .ToList();

        await membershipNotifier.NotifyMemberRemovedAsync(
            memberId,
            organization.Id,
            organization.Name,
            removedLabel,
            userId,
            performerDisplayName,
            auditRecipients,
            cancellationToken);
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

        await organizationMembershipRepository.TransferOwnership(
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

        var archivedOrganization = await organizationManagementRepository.Archive(
            organizationId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        var unarchivedOrganization = await organizationManagementRepository.Unarchive(
            organizationId, 
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        var isDeleted = await organizationManagementRepository.Delete(
            organizationId, 
            cancellationToken);
        if (!isDeleted)
            throw new NotFoundException("Organization not found.");

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationSlaPolicyDto> UpdateSlaPolicy(
        Guid userId,
        Guid organizationId,
        int lowHours,
        int mediumHours,
        int highHours,
        CancellationToken cancellationToken = default)
    {
        await policies.EnsureUserCanConfigureSlaPolicy(
            userId,
            organizationId,
            cancellationToken);

        ValidateSlaHours(lowHours, mediumHours, highHours);

        var organization = await organizationManagementRepository.UpdateSlaPolicy(
            organizationId,
            lowHours,
            mediumHours,
            highHours,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrganizationSlaPolicyDto(
            organization.SlaLowHours,
            organization.SlaMediumHours,
            organization.SlaHighHours);
    }

    private static void ValidateSlaHours(int lowHours, int mediumHours, int highHours)
    {
        EnsureSlaHourInRange(lowHours, nameof(lowHours));
        EnsureSlaHourInRange(mediumHours, nameof(mediumHours));
        EnsureSlaHourInRange(highHours, nameof(highHours));
    }

    private static void EnsureSlaHourInRange(int hours, string paramName)
    {
        if (hours is < 1 or > 8760)
            throw new BadRequestArgumentException(
                "Each SLA value must be between 1 and 8760 hours.",
                paramName);
    }
}