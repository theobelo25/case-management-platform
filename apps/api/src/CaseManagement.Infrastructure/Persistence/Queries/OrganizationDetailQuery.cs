using CaseManagement.Application.Organizations;
using CaseManagement.Application.Ports;
using Microsoft.EntityFrameworkCore;


namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class OrganizationDetailQuery(
    CaseManagementDbContext db,
    IOrganizationsRepository organizations
) : IOrganizationDetailQuery
{
    public async Task<OrganizationDetailDto?> GetDetailForMemberAsync(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        if (await organizations.CheckUserMembership(userId, organizationId, cancellationToken) is null)
            return null;

        var org = await db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
    
        if (org is null)
            return null;

        var rows = await db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId)
            .Join(
                db.Users.AsNoTracking(),
                m => m.UserId,
                u => u.Id,
                (m, u) => new {m.UserId, u.FirstName, u.LastName, m.Role})
            .ToListAsync(cancellationToken);
        
        var members = rows
            .Select(x => new OrganizationMemberDto(
                x.UserId,
                $"{x.FirstName} {x.LastName}".Trim(),
                x.Role.ToString()))
            .OrderBy(m => m.Name)
            .ToArray();
        
        return new OrganizationDetailDto(
            org.Id,
            org.Name,
            org.CreatedAtUtc,
            org.IsArchived,
            members);
    }
}