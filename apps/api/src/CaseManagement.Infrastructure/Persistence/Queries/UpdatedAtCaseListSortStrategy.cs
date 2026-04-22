using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class UpdatedAtCaseListSortStrategy(
    ICaseListCursorCodec cursorCodec) : ICaseListSortStrategy
{
    public IReadOnlyList<string> SortFieldKeys { get; } = ["UPDATED_AT"];

    public bool IsDefaultSortStrategy => true;

    public IQueryable<Case> ApplyCursor(IQueryable<Case> query, string? cursor, bool descending)
    {
        var decoded = cursorCodec.TryDecodeUpdatedAt(cursor);
        if (decoded is null)
            return query;

        var (updatedAtTicks, id) = decoded.Value;
        var updatedAt = new DateTimeOffset(updatedAtTicks, TimeSpan.Zero);
        return descending
            ? query.Where(c =>
                c.UpdatedAtUtc < updatedAt ||
                (c.UpdatedAtUtc == updatedAt && c.Id.CompareTo(id) < 0))
            : query.Where(c =>
                c.UpdatedAtUtc > updatedAt ||
                (c.UpdatedAtUtc == updatedAt && c.Id.CompareTo(id) > 0));
    }

    public IQueryable<Case> ApplySorting(IQueryable<Case> query, bool descending) =>
        descending
            ? query.OrderByDescending(c => c.UpdatedAtUtc).ThenByDescending(c => c.Id)
            : query.OrderBy(c => c.UpdatedAtUtc).ThenBy(c => c.Id);

    public string EncodeNextCursor(CaseListItemReadModel last) =>
        cursorCodec.EncodeUpdatedAt(last.UpdatedAtUtc, last.Id);
}
