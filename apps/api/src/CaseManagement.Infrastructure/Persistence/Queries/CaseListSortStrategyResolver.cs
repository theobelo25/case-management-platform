using System.Collections.ObjectModel;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseListSortStrategyResolver : ICaseListSortStrategyResolver
{
    private readonly ICaseListSortStrategy _defaultStrategy;
    private readonly IReadOnlyDictionary<string, ICaseListSortStrategy> _strategiesBySortField;

    public CaseListSortStrategyResolver(IEnumerable<ICaseListSortStrategy> strategies)
    {
        var list = strategies as IReadOnlyList<ICaseListSortStrategy> ?? strategies.ToList();

        _defaultStrategy = list.Single(s => s.IsDefaultSortStrategy);

        var map = new Dictionary<string, ICaseListSortStrategy>(StringComparer.OrdinalIgnoreCase);
        foreach (var strategy in list)
        {
            foreach (var key in strategy.SortFieldKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("Sort field keys must be non-empty.");

                if (!map.TryAdd(key.Trim(), strategy))
                    throw new InvalidOperationException(
                        $"Duplicate sort field key registration: '{key.Trim()}'.");
            }
        }

        _strategiesBySortField = new ReadOnlyDictionary<string, ICaseListSortStrategy>(map);
    }

    public ICaseListSortStrategy Resolve(string? sortField)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return _defaultStrategy;

        return _strategiesBySortField.TryGetValue(sortField.Trim(), out var strategy)
            ? strategy
            : _defaultStrategy;
    }
}
