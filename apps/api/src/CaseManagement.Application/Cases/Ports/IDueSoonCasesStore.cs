using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Ports;

public interface IDueSoonCasesStore
{
    Task<IReadOnlyList<Case>> GetOverdueCasesAsync(
        DateTimeOffset nowUtc,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Case>> GetDueSoonCandidatesAsync(
        DateTimeOffset nowUtc,
        DateTimeOffset dueBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task<bool> TryAddDueSoonNotificationMarkerAsync(
        Guid caseId,
        Guid recipientUserId,
        DateTimeOffset dueAtUtc,
        int windowMinutes,
        DateTimeOffset sentAtUtc,
        CancellationToken cancellationToken = default);
}
