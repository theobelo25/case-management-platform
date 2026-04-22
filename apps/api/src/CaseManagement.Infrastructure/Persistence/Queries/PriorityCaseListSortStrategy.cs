using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class PriorityCaseListSortStrategy(
    ICaseListCursorCodec cursorCodec) : ICaseListSortStrategy
{
    public IReadOnlyList<string> SortFieldKeys { get; } = ["PRIORITY"];

    public bool IsDefaultSortStrategy => false;

    public IQueryable<Case> ApplyCursor(IQueryable<Case> query, string? cursor, bool descending)
    {
        var decoded = cursorCodec.TryDecodePriority(cursor);
        if (decoded is null)
            return query;

        var (priority, id) = decoded.Value;

        return !descending
            ? query.Where(c =>
                (c.Priority == priority && c.Id.CompareTo(id) > 0) ||
                c.Priority > priority)
            : query.Where(c =>
                (c.Priority == priority && c.Id.CompareTo(id) > 0) ||
                c.Priority < priority);
    }

    public IQueryable<Case> ApplySorting(IQueryable<Case> query, bool descending) =>
        descending
            ? query.OrderByDescending(c => c.Priority).ThenBy(c => c.Id)
            : query.OrderBy(c => c.Priority).ThenBy(c => c.Id);

    public string EncodeNextCursor(CaseListItemReadModel last) =>
        cursorCodec.EncodePriority(last);
}
