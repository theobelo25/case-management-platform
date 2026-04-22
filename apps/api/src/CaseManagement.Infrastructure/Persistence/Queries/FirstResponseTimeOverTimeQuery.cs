using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class FirstResponseTimeOverTimeQuery(CaseManagementDbContext db) : IFirstResponseTimeOverTimeQuery
{
    public async Task<FirstResponseTimeOverTimeDto> GetAsync(
        Guid organizationId,
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = new DateTimeOffset(fromInclusive.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(toInclusive.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var caseRows = await (
                from c in db.Cases.AsNoTracking()
                join init in db.CaseMessages.AsNoTracking() on c.Id equals init.CaseId
                where init.IsInitial
                where c.OrganizationId == organizationId && !c.IsArchived
                where c.CreatedAtUtc >= fromUtc && c.CreatedAtUtc < toUtc
                group init by new { c.Id, c.CreatedAtUtc, c.RequesterUserId, c.AssigneeUserId } into g
                select new
                {
                    g.Key.Id,
                    g.Key.CreatedAtUtc,
                    g.Key.RequesterUserId,
                    g.Key.AssigneeUserId,
                    InitialAuthor = g.OrderBy(m => m.CreatedAtUtc).Select(m => m.AuthorUserId).First(),
                })
            .ToListAsync(cancellationToken);

        if (caseRows.Count == 0)
        {
            return new FirstResponseTimeOverTimeDto
            {
                Series = BuildEmptySeries(fromInclusive, toInclusive),
            };
        }

        var caseIds = caseRows.Select(r => r.Id).ToList();
        var messages = await db.CaseMessages.AsNoTracking()
            .Where(m => caseIds.Contains(m.CaseId) && !m.IsInitial)
            .Select(m => new { m.CaseId, m.CreatedAtUtc, m.IsInternal, m.AuthorUserId })
            .ToListAsync(cancellationToken);

        var messagesByCase = messages.GroupBy(m => m.CaseId).ToDictionary(g => g.Key, g => g.ToList());

        var minutesByDay = new Dictionary<DateOnly, List<double>>();
        foreach (var row in caseRows)
        {
            var compareAuthor = row.RequesterUserId ?? row.InitialAuthor;

            if (!messagesByCase.TryGetValue(row.Id, out var list))
                continue;

            // Qualifying "agent" reply: internal; or not from the requester/initial customer voice;
            // or from whoever is currently assigned (so assignee follow-ups always count toward FRT).
            var firstAgent = list
                .Where(m =>
                    m.IsInternal
                    || m.AuthorUserId != compareAuthor
                    || (row.AssigneeUserId is { } assigneeId && m.AuthorUserId == assigneeId))
                .OrderBy(m => m.CreatedAtUtc)
                .FirstOrDefault();

            if (firstAgent is null)
                continue;

            var minutes = (firstAgent.CreatedAtUtc - row.CreatedAtUtc).TotalMinutes;
            if (minutes < 0)
                continue;

            var day = DateOnly.FromDateTime(row.CreatedAtUtc.UtcDateTime);
            if (!minutesByDay.TryGetValue(day, out var bucket))
            {
                bucket = [];
                minutesByDay[day] = bucket;
            }

            bucket.Add(minutes);
        }

        var series = new List<FirstResponseTimeDayPointDto>();
        for (var d = fromInclusive; d <= toInclusive; d = d.AddDays(1))
        {
            if (!minutesByDay.TryGetValue(d, out var bucket) || bucket.Count == 0)
            {
                series.Add(new FirstResponseTimeDayPointDto
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    AverageFirstResponseMinutes = null,
                    CasesWithFirstResponse = 0,
                });
            }
            else
            {
                series.Add(new FirstResponseTimeDayPointDto
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    AverageFirstResponseMinutes = bucket.Average(),
                    CasesWithFirstResponse = bucket.Count,
                });
            }
        }

        return new FirstResponseTimeOverTimeDto { Series = series };
    }

    private static IReadOnlyList<FirstResponseTimeDayPointDto> BuildEmptySeries(
        DateOnly fromInclusive,
        DateOnly toInclusive)
    {
        var series = new List<FirstResponseTimeDayPointDto>();
        for (var d = fromInclusive; d <= toInclusive; d = d.AddDays(1))
        {
            series.Add(new FirstResponseTimeDayPointDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                AverageFirstResponseMinutes = null,
                CasesWithFirstResponse = 0,
            });
        }

        return series;
    }
}
