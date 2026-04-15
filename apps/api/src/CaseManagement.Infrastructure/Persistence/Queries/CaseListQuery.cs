using System.Text;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Common;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseListQuery(
    CaseManagementDbContext db) : ICaseListQuery
{
    private enum CaseSortField
    {
        UpdatedAt,
        Priority
    }

    public async Task<CursorPage<Case>> ExecuteAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default)
    {
        var organizationId = input.OrganizationId;
        var cursor = input.Cursor;
        var limit = input.Limit;
        var search = input.Filters.Search;
        var priority = input.Filters.Priority;
        var status = input.Filters.Status;
        var sort = input.Sort.Field;
        var descending = input.Sort.Descending;

        limit = Math.Clamp(limit, 1, 100);

        var query = db.Cases
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{term}%") ||
                (c.RequesterName != null && EF.Functions.ILike(c.RequesterName, $"%{term}%")));
        }

        var parsedPriority = ParsePriority(priority);
        if (parsedPriority is not null)
        {
            query = query.Where(c => c.Priority == parsedPriority.Value);
        }

        var statuses = ParseStatusFilter(status);
        if (statuses is { Length: > 0 })
        {
            query = query.Where(c => statuses.Contains(c.Status));
        }

        var sortField = ParseSortField(sort);

        if (sortField == CaseSortField.UpdatedAt)
        {
            var decoded = TryDecodeCursor(cursor);
            if (decoded is not null)
            {
                var (updatedAtTicks, id) = decoded.Value;
                var updatedAt = new DateTimeOffset(updatedAtTicks, TimeSpan.Zero);
                query = descending
                    ? query.Where(c =>
                        c.UpdatedAtUtc < updatedAt ||
                        (c.UpdatedAtUtc == updatedAt && c.Id.CompareTo(id) < 0))
                    : query.Where(c =>
                        c.UpdatedAtUtc > updatedAt ||
                        (c.UpdatedAtUtc == updatedAt && c.Id.CompareTo(id) > 0));
            }
        }

        query = ApplySorting(query, sortField, descending);

        var rows = await query
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > limit;
        var pageItems = hasMore ? rows.Take(limit).ToArray() : rows.ToArray();

        string nextCursor = string.Empty;
        if (hasMore && sortField == CaseSortField.UpdatedAt)
        {
            var last = pageItems[^1];
            nextCursor = EncodeCursor(last.UpdatedAtUtc, last.Id);
        }

        return new CursorPage<Case>(pageItems, nextCursor, limit);
    }

    private static IQueryable<Case> ApplySorting(
        IQueryable<Case> query,
        CaseSortField sortField,
        bool descending)
    {
        if (sortField == CaseSortField.Priority)
        {
            return descending
                ? query.OrderByDescending(c => c.Priority).ThenBy(c => c.Id)
                : query.OrderBy(c => c.Priority).ThenBy(c => c.Id);
        }

        return descending
            ? query.OrderByDescending(c => c.UpdatedAtUtc).ThenByDescending(c => c.Id)
            : query.OrderBy(c => c.UpdatedAtUtc).ThenBy(c => c.Id);
    }

    private static CaseSortField ParseSortField(string? sort)
    {
        if (string.Equals(sort, "PRIORITY", StringComparison.OrdinalIgnoreCase))
            return CaseSortField.Priority;

        return CaseSortField.UpdatedAt;
    }

    private static string EncodeCursor(DateTimeOffset createdAtUtc, Guid id)
    {
        var payload = $"{createdAtUtc.UtcTicks}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    private static CasePriority? ParsePriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return null;

        return priority.Trim().ToUpperInvariant() switch
        {
            "LOW" => CasePriority.Low,
            "MEDIUM" => CasePriority.Medium,
            "HIGH" => CasePriority.High,
            _ => null
        };
    }

    private static CaseStatus[]? ParseStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status.Trim().ToUpperInvariant() switch
        {
            "OPEN" => [CaseStatus.New, CaseStatus.Open],
            "IN_PROGRESS" => [CaseStatus.Pending],
            "CLOSED" => [CaseStatus.Resolved, CaseStatus.Closed],
            _ => null
        };
    }

    private static (long CreatedAtTicks, Guid Id)? TryDecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|');
            if (parts.Length != 2)
                return null;
            if (!long.TryParse(parts[0], out var ticks))
                return null;
            if (!Guid.TryParseExact(parts[1], "N", out var id))
                return null;
            return (ticks, id);
        }
        catch
        {
            return null;
        }
    }
}
