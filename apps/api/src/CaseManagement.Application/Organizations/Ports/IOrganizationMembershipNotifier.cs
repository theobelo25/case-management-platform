namespace CaseManagement.Application.Organizations.Ports;

/// <summary>
/// Pushes organization membership notifications to affected users (e.g. via SignalR).
/// </summary>
public interface IOrganizationMembershipNotifier
{
    Task NotifyMemberAddedAsync(
        Guid newMemberUserId,
        Guid organizationId,
        string organizationName,
        Guid performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default);

    /// <param name="auditRecipientUserIds">
    /// Owner and Admin users who should receive an audit line (excluding actor and removed user).
    /// </param>
    Task NotifyMemberRemovedAsync(
        Guid removedUserId,
        Guid organizationId,
        string organizationName,
        string removedMemberDisplayName,
        Guid performedByUserId,
        string? performedByDisplayName,
        IReadOnlyList<Guid> auditRecipientUserIds,
        CancellationToken cancellationToken = default);
}
