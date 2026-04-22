using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common.Ports;
using Microsoft.Extensions.Options;

namespace CaseManagement.Application.Cases.Services;

internal sealed class DueSoonCaseSelectionService(
    IDueSoonCasesStore store,
    IUnitOfWork unitOfWork,
    IOptions<DueSoonSchedulerOptions> options,
    TimeProvider timeProvider) : IDueSoonCaseSelectionService
{
    public async Task<DueSoonSelectionResult> SelectAsync(CancellationToken cancellationToken = default)
    {
        var startedAtUtc = timeProvider.GetUtcNow();
        var normalized = Normalize(options.Value);
        if (!normalized.Enabled || normalized.AllWindows.Length == 0)
            return new DueSoonSelectionResult(startedAtUtc, normalized, [], 0);

        var overdueCases = await store.GetOverdueCasesAsync(
            startedAtUtc,
            normalized.BatchSize,
            cancellationToken);

        var breachedThisRun = 0;
        foreach (var overdueCase in overdueCases)
        {
            if (!overdueCase.MarkSlaBreachedIfPastDue(startedAtUtc, "due_soon_scheduler"))
                continue;
            breachedThisRun++;
        }

        if (breachedThisRun > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        var dueBefore = startedAtUtc.AddMinutes(normalized.AllWindows[0]);
        var candidates = await store.GetDueSoonCandidatesAsync(
            startedAtUtc,
            dueBefore,
            normalized.BatchSize,
            cancellationToken);

        return new DueSoonSelectionResult(startedAtUtc, normalized, candidates, breachedThisRun);
    }

    private static DueSoonProcessingOptions Normalize(DueSoonSchedulerOptions raw)
    {
        var assignee = NormalizeWindows(raw.AssigneeWindowsMinutes, [240, 60, 15]);
        var privileged = NormalizeWindows(raw.PrivilegedWindowsMinutes, [15]);
        var all = assignee
            .Concat(privileged)
            .Distinct()
            .OrderByDescending(x => x)
            .ToArray();

        return new DueSoonProcessingOptions(
            raw.Enabled,
            Math.Max(1, raw.BatchSize),
            assignee,
            privileged,
            raw.NotifyRequester,
            all);
    }

    private static int[] NormalizeWindows(int[]? input, int[] fallback) =>
        (input is { Length: > 0 } ? input : fallback)
            .Where(x => x > 0)
            .Distinct()
            .OrderByDescending(x => x)
            .ToArray();
}
