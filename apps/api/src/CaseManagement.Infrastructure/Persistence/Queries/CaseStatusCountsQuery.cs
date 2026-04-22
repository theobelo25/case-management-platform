using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseStatusCountsQuery(CaseManagementDbContext db) : ICaseStatusCountsQuery
{
    public async Task<CaseStatusCountsDto> GetAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var rows = await db.Cases
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && !c.IsArchived)
            .GroupBy(c => c.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var map = rows.ToDictionary(x => x.Key, x => x.Count);

        static int Get(IReadOnlyDictionary<CaseStatus, int> m, CaseStatus s) =>
            m.GetValueOrDefault(s);

        return new CaseStatusCountsDto(
            Get(map, CaseStatus.New),
            Get(map, CaseStatus.Open),
            Get(map, CaseStatus.Pending),
            Get(map, CaseStatus.Resolved),
            Get(map, CaseStatus.Closed));
    }
}
