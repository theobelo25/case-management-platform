using CaseManagement.Application.Cases;
using CaseManagement.Application.Common;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseListQuery(
    CaseManagementDbContext db,
    ICaseListFilterApplier filterApplier,
    ICaseListSortStrategyResolver sortStrategyResolver) : ICaseListQuery
{
    public async Task<CursorPage<CaseListItemReadModel>> ExecuteAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default)
    {
        var cursor = input.Cursor;
        var limit = input.Limit;
        var sort = input.Sort.Field;
        var descending = input.Sort.Descending;
        var nowUtc = DateTimeOffset.UtcNow;

        limit = Math.Clamp(limit, 1, 100);

        var query = db.Cases
            .AsNoTracking();
        query = filterApplier.Apply(query, input, nowUtc);

        var sortStrategy = sortStrategyResolver.Resolve(sort);
        query = sortStrategy.ApplyCursor(query, cursor, descending);
        query = sortStrategy.ApplySorting(query, descending);

        var rows = await query
            .Select(c => new CaseListItemReadModel(
                c.Id,
                c.Title,
                c.Status,
                c.Priority,
                c.SlaDueAtUtc,
                c.SlaBreachedAtUtc,
                c.SlaPausedAtUtc,
                c.SlaRemainingSeconds,
                c.RequesterUserId,
                c.RequesterName,
                c.AssigneeUserId,
                c.CreatedByUserId,
                c.CreatedAtUtc,
                c.UpdatedAtUtc))
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > limit;
        var pageItems = hasMore ? rows.Take(limit).ToArray() : rows.ToArray();

        string nextCursor = string.Empty;
        if (hasMore)
        {
            var last = pageItems[^1];
            nextCursor = sortStrategy.EncodeNextCursor(last);
        }

        return new CursorPage<CaseListItemReadModel>(pageItems, nextCursor, limit);
    }
}
