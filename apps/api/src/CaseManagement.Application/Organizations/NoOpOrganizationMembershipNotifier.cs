using CaseManagement.Application.Organizations.Ports;

namespace CaseManagement.Application.Organizations;

internal sealed class NoOpOrganizationMembershipNotifier : IOrganizationMembershipNotifier
{
    public Task NotifyMemberAddedAsync(
        Guid newMemberUserId,
        Guid organizationId,
        string organizationName,
        Guid performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifyMemberRemovedAsync(
        Guid removedUserId,
        Guid organizationId,
        string organizationName,
        string removedMemberDisplayName,
        Guid performedByUserId,
        string? performedByDisplayName,
        IReadOnlyList<Guid> auditRecipientUserIds,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
