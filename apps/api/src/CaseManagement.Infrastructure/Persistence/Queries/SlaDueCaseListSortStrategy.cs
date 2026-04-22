using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class SlaDueCaseListSortStrategy(
    ICaseListCursorCodec cursorCodec) : ICaseListSortStrategy
{
    public IReadOnlyList<string> SortFieldKeys { get; } = ["SLA_DUE", "NEAREST_DUE"];

    public bool IsDefaultSortStrategy => false;

    public IQueryable<Case> ApplyCursor(IQueryable<Case> query, string? cursor, bool descending)
    {
        var decoded = cursorCodec.TryDecodeSla(cursor);
        if (decoded is null)
            return query;

        var (hasDue, dueAt, id) = decoded.Value;

        if (!descending)
        {
            if (hasDue && dueAt is { } due)
            {
                return query.Where(c =>
                    (c.SlaDueAtUtc != null &&
                        (c.SlaDueAtUtc.Value > due ||
                            (c.SlaDueAtUtc.Value == due && c.Id.CompareTo(id) > 0)))
                    || c.SlaDueAtUtc == null);
            }

            return query.Where(c => c.SlaDueAtUtc == null && c.Id.CompareTo(id) > 0);
        }

        if (hasDue && dueAt is { } dueDesc)
        {
            return query.Where(c =>
                (c.SlaDueAtUtc != null &&
                    (c.SlaDueAtUtc.Value < dueDesc ||
                        (c.SlaDueAtUtc.Value == dueDesc && c.Id.CompareTo(id) < 0)))
                || c.SlaDueAtUtc == null);
        }

        return query.Where(c => c.SlaDueAtUtc == null && c.Id.CompareTo(id) < 0);
    }

    public IQueryable<Case> ApplySorting(IQueryable<Case> query, bool descending) =>
        descending
            ? query
                .OrderBy(c => c.SlaDueAtUtc == null)
                .ThenByDescending(c => c.SlaDueAtUtc)
                .ThenByDescending(c => c.Id)
            : query
                .OrderBy(c => c.SlaDueAtUtc == null)
                .ThenBy(c => c.SlaDueAtUtc)
                .ThenBy(c => c.Id);

    public string EncodeNextCursor(CaseListItemReadModel last) =>
        cursorCodec.EncodeSla(last);
}
