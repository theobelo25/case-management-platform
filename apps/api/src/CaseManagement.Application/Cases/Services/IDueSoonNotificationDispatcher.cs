using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal interface IDueSoonNotificationDispatcher
{
    Task DispatchAsync(
        Case @case,
        int windowMinutes,
        IReadOnlyList<Guid> recipientIds,
        CancellationToken cancellationToken = default);
}
