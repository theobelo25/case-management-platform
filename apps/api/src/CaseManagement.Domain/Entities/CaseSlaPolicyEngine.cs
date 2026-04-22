namespace CaseManagement.Domain.Entities;

internal readonly record struct CaseSlaState(
    DateTimeOffset? DueAtUtc,
    DateTimeOffset? BreachedAtUtc,
    DateTimeOffset? PausedAtUtc,
    int? RemainingSeconds);

internal readonly record struct CaseSlaTransition(
    CaseSlaState State,
    bool DueChanged,
    bool BreachedNow,
    bool BreachCleared);

internal static class CaseSlaPolicyEngine
{
    public static CaseSlaTransition Recalculate(
        CaseStatus status,
        CasePriority priority,
        DateTimeOffset nowUtc,
        CaseSlaState state,
        CasePriority? previousPriority = null,
        SlaDurationPolicy? slaPolicy = null)
    {
        var dueAtUtc = state.DueAtUtc;
        var breachedAtUtc = state.BreachedAtUtc;
        var pausedAtUtc = state.PausedAtUtc;
        var remainingSeconds = state.RemainingSeconds;

        var previousDue = dueAtUtc;
        var previousBreachedAt = breachedAtUtc;

        if (IsTerminal(status))
        {
            if (pausedAtUtc is not null &&
                remainingSeconds is null &&
                dueAtUtc is not null)
            {
                remainingSeconds = Math.Max(0, (int)(dueAtUtc.Value - pausedAtUtc.Value).TotalSeconds);
            }

            pausedAtUtc = null;
            dueAtUtc = remainingSeconds.HasValue ? nowUtc.AddSeconds(remainingSeconds.Value) : dueAtUtc;
        }
        else if (ShouldPauseClock(status))
        {
            if (pausedAtUtc is null)
            {
                remainingSeconds = dueAtUtc.HasValue
                    ? Math.Max(0, (int)(dueAtUtc.Value - nowUtc).TotalSeconds)
                    : null;
                pausedAtUtc = nowUtc;
            }
        }
        else
        {
            if (pausedAtUtc is not null && remainingSeconds is not null)
            {
                dueAtUtc = nowUtc.AddSeconds(remainingSeconds.Value);
                pausedAtUtc = null;
                remainingSeconds = null;
            }

            if (previousPriority.HasValue && previousPriority.Value != priority)
            {
                if (!slaPolicy.HasValue)
                    throw new InvalidOperationException("SLA policy is required when priority changes.");

                var duration = slaPolicy.Value.ForPriority(priority);
                dueAtUtc = nowUtc.Add(duration);
            }
        }

        var breachedNow = false;
        var breachCleared = false;
        if (dueAtUtc.HasValue && dueAtUtc.Value <= nowUtc && breachedAtUtc is null)
        {
            breachedAtUtc = nowUtc;
            breachedNow = true;
        }
        else if (dueAtUtc.HasValue && dueAtUtc.Value > nowUtc && previousBreachedAt is not null)
        {
            breachedAtUtc = null;
            breachCleared = true;
        }

        return new CaseSlaTransition(
            new CaseSlaState(dueAtUtc, breachedAtUtc, pausedAtUtc, remainingSeconds),
            dueAtUtc != previousDue,
            breachedNow,
            breachCleared);
    }

    private static bool IsTerminal(CaseStatus status) =>
        status is CaseStatus.Resolved or CaseStatus.Closed;

    private static bool ShouldPauseClock(CaseStatus status) =>
        status == CaseStatus.Pending;
}
