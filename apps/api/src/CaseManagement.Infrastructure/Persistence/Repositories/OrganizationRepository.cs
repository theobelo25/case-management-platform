using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(
    CaseManagementDbContext db,
    IUnitOfWork unitOfWork) : IOrganizationsRepository
{
    public Task<Organization> Create(
        string name,
        CancellationToken cancellationToken = default)
    {
        var organization = Organization.Create(name);
        db.Organizations.Add(organization);

        return Task.FromResult(organization);
    }

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

    public async Task<OrganizationRole?> CheckUserMembership(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await db.OrganizationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.OrganizationId == organizationId,
                cancellationToken);

        return membership?.Role;
    }

    public async Task<Organization> Archive(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");
        
        organization.Archive();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization;
    }

    public async Task<Organization> Unarchive(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");
        
        organization.Unarchive();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization;
    }

    public async Task<bool> Delete(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var deleted = await db.Organizations
            .Where(o => o.Id == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }
}