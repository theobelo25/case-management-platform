using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Api.Controllers;

internal static class CasesRequestParser
{
    /// <summary>
    /// Ordered wire names for bulk actions (single source for validation messages and parsing).
    /// </summary>
    private static readonly (string Wire, BulkCaseAction Action)[] BulkCaseActionDefinitions =
    [
        ("ASSIGN", BulkCaseAction.Assign),
        ("SET_PRIORITY", BulkCaseAction.SetPriority),
        ("SET_STATUS", BulkCaseAction.SetStatus),
        ("BUMP_PRIORITY", BulkCaseAction.BumpPriority),
    ];

    private static readonly Dictionary<string, BulkCaseAction> BulkCaseActionsByWireName =
        BulkCaseActionDefinitions.ToDictionary(
            x => x.Wire,
            x => x.Action,
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Allowed case list sort field wire names (single source for validation and error text).
    /// </summary>
    private static readonly string[] CaseListSortWireNames =
    [
        "UPDATED_AT",
        "PRIORITY",
        "SLA_DUE",
        "NEAREST_DUE",
    ];

    private static readonly HashSet<string> CaseListSortWireNamesSet =
        new(CaseListSortWireNames, StringComparer.OrdinalIgnoreCase);

    public static bool TryParseBulkCaseAction(string? value, out BulkCaseAction action)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            action = default;
            return false;
        }

        return BulkCaseActionsByWireName.TryGetValue(value.Trim(), out action);
    }

    /// <summary>
    /// Comma-separated wire names as used in validation error messages.
    /// </summary>
    public static string SupportedBulkCaseActionWireNamesForMessage =>
        string.Join(", ", BulkCaseActionDefinitions.Select(x => x.Wire));

    public static bool TryParseCaseListSort(string? sort, out string? normalizedSort, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            normalizedSort = null;
            errorMessage = string.Empty;
            return true;
        }

        var trimmed = sort.Trim();
        if (CaseListSortWireNamesSet.Contains(trimmed))
        {
            normalizedSort = trimmed;
            errorMessage = string.Empty;
            return true;
        }

        normalizedSort = null;
        errorMessage = $"Sort must be omitted or one of: {string.Join(", ", CaseListSortWireNames)}.";
        return false;
    }
}
