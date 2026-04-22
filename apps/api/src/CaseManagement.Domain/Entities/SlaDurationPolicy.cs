namespace CaseManagement.Domain.Entities;

/// <summary>
/// Per-priority response-time targets used when opening a case or when priority changes.
/// </summary>
public readonly record struct SlaDurationPolicy(int LowHours, int MediumHours, int HighHours)
{
    public TimeSpan ForPriority(CasePriority priority) =>
        TimeSpan.FromHours(priority switch
        {
            CasePriority.Low => LowHours,
            CasePriority.Medium => MediumHours,
            CasePriority.High => HighHours,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
        });
}
