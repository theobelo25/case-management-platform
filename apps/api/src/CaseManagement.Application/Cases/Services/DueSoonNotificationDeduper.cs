using CaseManagement.Application.Cases.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed class DueSoonNotificationDeduper(
    IDueSoonCasesStore store) : IDueSoonNotificationDeduper
{
    public async Task<(IReadOnlyList<Guid> RecipientIdsToNotify, int DedupedCount)> DeduplicateRecipientsAsync(
        Case @case,
        IReadOnlyCollection<Guid> recipientIds,
        DateTimeOffset dueAtUtc,
        int windowMinutes,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var recipientIdsToNotify = new List<Guid>(recipientIds.Count);
        var dedupedCount = 0;

        foreach (var recipientId in recipientIds)
        {
            var saved = await store.TryAddDueSoonNotificationMarkerAsync(
                @case.Id,
                recipientId,
                dueAtUtc,
                windowMinutes,
                startedAtUtc,
                cancellationToken);

            if (saved)
            {
                recipientIdsToNotify.Add(recipientId);
                continue;
            }

            dedupedCount++;
        }

        return (recipientIdsToNotify, dedupedCount);
    }
}
