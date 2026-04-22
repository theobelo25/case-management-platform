using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class OrganizationPrivilegedUserIdsQuery(CaseManagementDbContext db)
    : IOrganizationPrivilegedUserIdsQuery
{
    public async Task<IReadOnlyList<Guid>> GetOwnerAndAdminUserIdsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var list = await db.OrganizationMemberships
            .AsNoTracking()
            .Where(m =>
                m.OrganizationId == organizationId &&
                (m.Role == OrganizationRole.Owner || m.Role == OrganizationRole.Admin))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        return list;
    }
}
