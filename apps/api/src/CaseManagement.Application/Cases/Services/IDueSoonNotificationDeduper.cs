using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal interface IDueSoonNotificationDeduper
{
    Task<(IReadOnlyList<Guid> RecipientIdsToNotify, int DedupedCount)> DeduplicateRecipientsAsync(
        Case @case,
        IReadOnlyCollection<Guid> recipientIds,
        DateTimeOffset dueAtUtc,
        int windowMinutes,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken = default);
}
