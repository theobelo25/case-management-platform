using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseVolumeOverTimeQuery(CaseManagementDbContext db) : ICaseVolumeOverTimeQuery
{
    public async Task<CaseVolumeOverTimeDto> GetAsync(
        Guid organizationId,
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = new DateTimeOffset(fromInclusive.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(toInclusive.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var createdTimes = await db.Cases
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && !c.IsArchived)
            .Where(c => c.CreatedAtUtc >= fromUtc && c.CreatedAtUtc < toUtc)
            .Select(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var statusEventTimes = await (
                from e in db.CaseEvents.AsNoTracking()
                join c in db.Cases.AsNoTracking() on e.CaseId equals c.Id
                where c.OrganizationId == organizationId && !c.IsArchived
                where e.Type == "status_changed"
                where e.CreatedAtUtc >= fromUtc && e.CreatedAtUtc < toUtc
                where e.MetadataJson != null
                select new { e.MetadataJson, e.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var createdByDay = CountByUtcDay(createdTimes);
        var resolvedByDay = new Dictionary<DateOnly, int>();
        var reopenedByDay = new Dictionary<DateOnly, int>();

        foreach (var row in statusEventTimes)
        {
            var json = row.MetadataJson!;
            var day = DateOnly.FromDateTime(row.CreatedAtUtc.UtcDateTime);

            if (json.Contains("\"to\":\"Resolved\"", StringComparison.Ordinal))
                Add(resolvedByDay, day, 1);

            var fromTerminal =
                json.Contains("\"from\":\"Resolved\"", StringComparison.Ordinal) ||
                json.Contains("\"from\":\"Closed\"", StringComparison.Ordinal);
            var toActive =
                json.Contains("\"to\":\"New\"", StringComparison.Ordinal) ||
                json.Contains("\"to\":\"Open\"", StringComparison.Ordinal) ||
                json.Contains("\"to\":\"Pending\"", StringComparison.Ordinal);
            if (fromTerminal && toActive)
                Add(reopenedByDay, day, 1);
        }

        var series = new List<CaseVolumeDayPointDto>();
        for (var d = fromInclusive; d <= toInclusive; d = d.AddDays(1))
        {
            series.Add(new CaseVolumeDayPointDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                CasesCreated = createdByDay.GetValueOrDefault(d),
                CasesResolved = resolvedByDay.GetValueOrDefault(d),
                CasesReopened = reopenedByDay.GetValueOrDefault(d),
            });
        }

        return new CaseVolumeOverTimeDto { Series = series };
    }

    private static Dictionary<DateOnly, int> CountByUtcDay(IReadOnlyCollection<DateTimeOffset> times)
    {
        var map = new Dictionary<DateOnly, int>();
        foreach (var t in times)
        {
            var day = DateOnly.FromDateTime(t.UtcDateTime);
            Add(map, day, 1);
        }

        return map;
    }

    private static void Add(Dictionary<DateOnly, int> map, DateOnly day, int delta)
    {
        map[day] = map.GetValueOrDefault(day) + delta;
    }
}
