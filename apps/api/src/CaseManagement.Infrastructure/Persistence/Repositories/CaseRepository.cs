using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Domain.Entities;
using CaseManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class CaseRepository(
    CaseManagementDbContext db
) : ICaseRepository
{
    public void Add(
        Case @case) =>
        db.Cases.Add(@case);

    public void Remove(
        Case @case) =>
        db.Cases.Remove(@case);

    public Task<Case?> GetById(
        Guid caseId,
        CancellationToken cancellationToken = default) =>
        db.Cases
            .Include(c => c.Messages)
            .Include(c => c.Events)
            .SingleOrDefaultAsync(c => c.Id == caseId, cancellationToken);

    public async Task<IReadOnlyList<Case>> GetByIdsInOrganizationAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid> caseIds,
        CancellationToken cancellationToken = default)
    {
        if (caseIds.Count == 0)
            return Array.Empty<Case>();

        var distinctIds = caseIds.Distinct().ToList();
        var list = await db.Cases
            .Include(c => c.Messages)
            .Include(c => c.Events)
            .Where(c => c.OrganizationId == organizationId && distinctIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        return list;
    }

    public Task<int> UpdateStatusAndPriorityAsync(
        Guid caseId,
        CaseStatus status,
        CasePriority priority,
        CancellationToken cancellationToken = default) =>
        db.Cases
            .Where(c => c.Id == caseId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(c => c.Status, status)
                    .SetProperty(c => c.Priority, priority)
                    .SetProperty(c => c.UpdatedAtUtc, DateTimeOffset.UtcNow),
                cancellationToken);
}