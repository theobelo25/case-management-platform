using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

public static class DueSoonWindowMatcher
{
    public static bool IsSuppressed(Case @case) =>
        @case.IsArchived
        || @case.Status is CaseStatus.Resolved or CaseStatus.Closed
        || @case.SlaDueAtUtc is null
        || @case.AssigneeUserId is null
        || @case.SlaPausedAtUtc is not null
        || @case.SlaBreachedAtUtc is not null;

    public static int? MatchWindow(
        DateTimeOffset nowUtc,
        DateTimeOffset dueAtUtc,
        IReadOnlyList<int> sortedDescendingWindows)
    {
        if (sortedDescendingWindows.Count == 0)
            return null;

        var remaining = dueAtUtc - nowUtc;
        if (remaining <= TimeSpan.Zero)
            return null;

        var remainingMinutes = remaining.TotalMinutes;

        for (var i = 0; i < sortedDescendingWindows.Count; i++)
        {
            var upper = sortedDescendingWindows[i];
            var lower = i == sortedDescendingWindows.Count - 1
                ? 0
                : sortedDescendingWindows[i + 1];

            if (remainingMinutes <= upper && remainingMinutes > lower)
                return upper;
        }

        return null;
    }
}
