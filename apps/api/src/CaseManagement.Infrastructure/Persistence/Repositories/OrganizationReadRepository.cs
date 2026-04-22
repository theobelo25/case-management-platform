using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class OrganizationReadRepository(
    CaseManagementDbContext db) : IOrganizationReadRepository
{
    public Task<Organization?> GetById(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        return organization;
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

    public async Task<IReadOnlyList<Guid>> GetOwnerAndAdminUserIds(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await db.OrganizationMemberships
            .AsNoTracking()
            .Where(m =>
                m.OrganizationId == organizationId &&
                (m.Role == OrganizationRole.Owner || m.Role == OrganizationRole.Admin))
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
    }
}
