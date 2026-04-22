using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;

namespace CaseManagement.Application.Cases.Services;

internal sealed class DueSoonNotificationProcessor(
    IDueSoonCaseSelectionService selector,
    IDueSoonRecipientResolver recipientsResolver,
    IDueSoonNotificationDeduper deduper,
    IDueSoonNotificationDispatcher dispatcher)
    : IDueSoonNotificationProcessor
{
    public async Task<DueSoonRunResult> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var selection = await selector.SelectAsync(cancellationToken);
        if (!selection.Options.Enabled || selection.Options.AllWindows.Length == 0)
            return DueSoonRunResult.Disabled;

        var result = new DueSoonRunResult
        {
            Scanned = selection.Candidates.Count,
            Breached = selection.BreachedCount
        };

        foreach (var @case in selection.Candidates)
        {
            try
            {
                if (DueSoonWindowMatcher.IsSuppressed(@case))
                {
                    result.Skipped++;
                    continue;
                }

                var dueAtUtc = @case.SlaDueAtUtc!.Value;
                var windowMinutes = DueSoonWindowMatcher.MatchWindow(
                    selection.StartedAtUtc,
                    dueAtUtc,
                    selection.Options.AllWindows);

                if (windowMinutes is null)
                {
                    result.Skipped++;
                    continue;
                }

                var recipients = await recipientsResolver.ResolveRecipientsAsync(
                    @case,
                    windowMinutes.Value,
                    selection.Options,
                    cancellationToken);

                if (recipients.Count == 0)
                {
                    result.Skipped++;
                    continue;
                }

                var (recipientIdsToNotify, dedupedCount) = await deduper.DeduplicateRecipientsAsync(
                    @case,
                    recipients,
                    dueAtUtc,
                    windowMinutes.Value,
                    selection.StartedAtUtc,
                    cancellationToken);
                result.Deduped += dedupedCount;

                if (recipientIdsToNotify.Count == 0)
                    continue;

                await dispatcher.DispatchAsync(
                    @case,
                    windowMinutes.Value,
                    recipientIdsToNotify,
                    cancellationToken);

                result.Notified += recipientIdsToNotify.Count;
            }
            catch
            {
                result.Errors++;
            }
        }

        return result;
    }
}
