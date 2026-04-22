using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases;

public static class CaseStatusPriorityMapper
{
    private static readonly Dictionary<CasePriorityCode, CasePriority> DomainPriorityByCode = new()
    {
        [CasePriorityCode.LOW] = CasePriority.Low,
        [CasePriorityCode.MEDIUM] = CasePriority.Medium,
        [CasePriorityCode.HIGH] = CasePriority.High,
    };

    private static readonly Dictionary<CaseStatusCode, CaseStatus> DomainStatusByCode = new()
    {
        [CaseStatusCode.NEW] = CaseStatus.New,
        [CaseStatusCode.OPEN] = CaseStatus.Open,
        [CaseStatusCode.PENDING] = CaseStatus.Pending,
        [CaseStatusCode.RESOLVED] = CaseStatus.Resolved,
        [CaseStatusCode.CLOSED] = CaseStatus.Closed,
    };

    private static readonly Dictionary<CasePriority, string> ApiPriorityCodeByDomain = new()
    {
        [CasePriority.Low] = CasePriorityCode.LOW.ToString(),
        [CasePriority.Medium] = CasePriorityCode.MEDIUM.ToString(),
        [CasePriority.High] = CasePriorityCode.HIGH.ToString(),
    };

    private static readonly Dictionary<string, CaseStatus[]> StatusFilterByWireName =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["NEW"] = [CaseStatus.New],
            ["OPEN"] = [CaseStatus.New, CaseStatus.Open],
            ["PENDING"] = [CaseStatus.Pending],
            ["IN_PROGRESS"] = [CaseStatus.Pending],
            ["RESOLVED"] = [CaseStatus.Resolved],
            ["CLOSED"] = [CaseStatus.Resolved, CaseStatus.Closed],
        };

    private static readonly HashSet<string> CaseStatusCodeWireNames =
        new(Enum.GetNames<CaseStatusCode>(), StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> CasePriorityCodeWireNames =
        new(Enum.GetNames<CasePriorityCode>(), StringComparer.OrdinalIgnoreCase);

    public static CasePriority ToDomainPriority(CasePriorityCode priority) =>
        DomainPriorityByCode.TryGetValue(priority, out var p)
            ? p
            : throw new ArgumentOutOfRangeException(nameof(priority), priority, null);

    public static CaseStatus ToDomainStatus(CaseStatusCode status) =>
        DomainStatusByCode.TryGetValue(status, out var s)
            ? s
            : throw new ArgumentOutOfRangeException(nameof(status), status, null);

    public static string ToApiPriorityCode(CasePriority priority) =>
        ApiPriorityCodeByDomain.TryGetValue(priority, out var code)
            ? code
            : throw new ArgumentOutOfRangeException(nameof(priority), priority, null);

    /// <summary>
    /// Parses API wire names that match <see cref="CaseStatusCode"/> (case-insensitive). Numeric strings are rejected.
    /// </summary>
    public static bool TryParseStatusCode(string? value, out CaseStatusCode code)
    {
        code = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (!CaseStatusCodeWireNames.Contains(trimmed))
            return false;

        return Enum.TryParse(trimmed, ignoreCase: true, out code);
    }

    /// <summary>
    /// Parses API wire names that match <see cref="CasePriorityCode"/> (case-insensitive). Numeric strings are rejected.
    /// </summary>
    public static bool TryParsePriorityCode(string? value, out CasePriorityCode code)
    {
        code = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (!CasePriorityCodeWireNames.Contains(trimmed))
            return false;

        return Enum.TryParse(trimmed, ignoreCase: true, out code);
    }

    public static CasePriority? ParsePriorityFilter(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : TryParsePriorityCode(value, out var code)
                ? ToDomainPriority(code)
                : null;

    public static CaseStatus[]? ParseStatusFilter(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : StatusFilterByWireName.TryGetValue(value.Trim(), out var statuses)
                ? statuses
                : null;
}
