using CaseManagement.Application.Organizations;
using CaseManagement.Application.Organizations.Ports;
using Microsoft.EntityFrameworkCore;


namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class OrganizationDetailQuery(
    CaseManagementDbContext db
) : IOrganizationDetailQuery
{
    public async Task<OrganizationDetailDto?> GetDetailForMemberAsync(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        var isMember = await db.OrganizationMemberships
            .AsNoTracking()
            .AnyAsync(
                m => m.UserId == userId && m.OrganizationId == organizationId,
                cancellationToken);
        if (!isMember)
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
                (m, u) => new {
                    m.UserId, 
                    u.FirstName, 
                    u.LastName, 
                    m.Role,
                    u.EmailNormalized,
                    m.CreatedAtUtc})
            .ToListAsync(cancellationToken);
        
        var members = rows
            .Select(x => new OrganizationMemberDto(
                x.UserId,
                $"{x.FirstName} {x.LastName}".Trim(),
                x.Role.ToString(),
                x.EmailNormalized,
                x.CreatedAtUtc))
            .OrderBy(m => m.Name)
            .ToArray();
        
        return new OrganizationDetailDto(
            org.Id,
            org.Name,
            org.CreatedAtUtc,
            org.IsArchived,
            org.SlaLowHours,
            org.SlaMediumHours,
            org.SlaHighHours,
            members);
    }
}