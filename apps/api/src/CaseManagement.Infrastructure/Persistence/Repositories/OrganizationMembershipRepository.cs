using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class OrganizationMembershipRepository(
    CaseManagementDbContext db) : IOrganizationMembershipRepository
{
    public Task<OrganizationMembership> IssueMembership(
        Guid userId,
        Guid organizationId,
        OrganizationRole role,
        CancellationToken cancellationToken = default)
    {
        var membership = new OrganizationMembership(
            organizationId,
            userId,
            role);

        db.OrganizationMemberships.Add(membership);

        return Task.FromResult(membership);
    }

    public async Task<int> RevokeMembership(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await db.OrganizationMemberships
            .FirstOrDefaultAsync(
                om => om.OrganizationId == organizationId && om.UserId == userId,
                cancellationToken);

        if (membership is null)
            return 0;

        db.OrganizationMemberships.Remove(membership);
        return 1;
    }

    public async Task TransferOwnership(
        Guid userId,
        Guid memberId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var currentOwnerMembership = await db.OrganizationMemberships
            .FirstOrDefaultAsync(
                cm => cm.OrganizationId == organizationId
                    && cm.UserId == userId
                    && cm.Role == OrganizationRole.Owner,
                cancellationToken)
            ?? throw new NotFoundException("Current owner membership not found.");

        var newOwnerMembership = await db.OrganizationMemberships
            .FirstOrDefaultAsync(
                nm => nm.OrganizationId == organizationId
                    && nm.UserId == memberId,
                cancellationToken)
            ?? throw new NotFoundException("User is not a member of this organization.");

        currentOwnerMembership.ChangeRole(OrganizationRole.Member);
        newOwnerMembership.ChangeRole(OrganizationRole.Owner);
    }
}
