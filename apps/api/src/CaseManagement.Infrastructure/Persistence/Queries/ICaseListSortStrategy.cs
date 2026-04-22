using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public interface ICaseListSortStrategy
{
    /// <summary>
    /// Wire/API sort field names this strategy handles (case-insensitive). Multiple aliases may map to one strategy.
    /// </summary>
    IReadOnlyList<string> SortFieldKeys { get; }

    /// <summary>
    /// When true, this strategy is used when sort is omitted or does not match any <see cref="SortFieldKeys"/>.
    /// Exactly one registered strategy must be the default.
    /// </summary>
    bool IsDefaultSortStrategy { get; }

    IQueryable<Case> ApplyCursor(IQueryable<Case> query, string? cursor, bool descending);
    IQueryable<Case> ApplySorting(IQueryable<Case> query, bool descending);
    string EncodeNextCursor(CaseListItemReadModel last);
}
