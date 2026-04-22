using CaseManagement.Application.Cases.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class DueSoonCasesStore(CaseManagementDbContext db)
    : IDueSoonCasesStore
{
    public async Task<IReadOnlyList<Case>> GetOverdueCasesAsync(
        DateTimeOffset nowUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var list = await db.Cases
            .Where(c =>
                !c.IsArchived
                && c.Status != CaseStatus.Resolved
                && c.Status != CaseStatus.Closed
                && c.SlaDueAtUtc != null
                && c.SlaPausedAtUtc == null
                && c.SlaBreachedAtUtc == null
                && c.SlaDueAtUtc <= nowUtc)
            .OrderBy(c => c.SlaDueAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task<IReadOnlyList<Case>> GetDueSoonCandidatesAsync(
        DateTimeOffset nowUtc,
        DateTimeOffset dueBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var list = await db.Cases
            .Where(c =>
                !c.IsArchived
                && c.Status != CaseStatus.Resolved
                && c.Status != CaseStatus.Closed
                && c.SlaDueAtUtc != null
                && c.AssigneeUserId != null
                && c.SlaPausedAtUtc == null
                && c.SlaBreachedAtUtc == null
                && c.SlaDueAtUtc > nowUtc
                && c.SlaDueAtUtc <= dueBeforeUtc)
            .OrderBy(c => c.SlaDueAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task<bool> TryAddDueSoonNotificationMarkerAsync(
        Guid caseId,
        Guid recipientUserId,
        DateTimeOffset dueAtUtc,
        int windowMinutes,
        DateTimeOffset sentAtUtc,
        CancellationToken cancellationToken = default)
    {
        var marker = CaseDueSoonNotification.Create(
            caseId,
            recipientUserId,
            dueAtUtc,
            windowMinutes,
            sentAtUtc);

        db.CaseDueSoonNotifications.Add(marker);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            db.Entry(marker).State = EntityState.Detached;
            return false;
        }
        catch (DbUpdateException)
        {
            db.Entry(marker).State = EntityState.Detached;
            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        if (string.IsNullOrWhiteSpace(message))
            return false;

        return message.Contains("duplicate key value violates unique constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
