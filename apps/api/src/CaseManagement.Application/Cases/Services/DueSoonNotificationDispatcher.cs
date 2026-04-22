using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed class DueSoonNotificationDispatcher(
    ICaseNotificationPublisher notifications) : IDueSoonNotificationDispatcher
{
    public async Task DispatchAsync(
        Case @case,
        int windowMinutes,
        IReadOnlyList<Guid> recipientIds,
        CancellationToken cancellationToken = default)
    {
        if (recipientIds.Count == 0)
            return;

        await notifications.NotifySlaDueSoonAsync(
            new CaseRef(@case.OrganizationId, @case.Id, @case.Title),
            windowMinutes,
            recipientIds,
            cancellationToken);
    }
}
