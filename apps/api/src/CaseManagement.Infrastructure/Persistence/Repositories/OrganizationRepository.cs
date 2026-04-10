using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(
    CaseManagementDbContext db,
    IUnitOfWork unitOfWork) : IOrganizationRepository
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
}