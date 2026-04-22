using CaseManagement.Application.Organizations.Ports;
using Microsoft.AspNetCore.SignalR;

namespace CaseManagement.Api.Realtime;

public sealed class OrganizationMembershipSignalRNotifier(
    IHubContext<NotificationsHub, INotificationsClient> hubContext
) : IOrganizationMembershipNotifier
{
    public async Task NotifyMemberAddedAsync(
        Guid newMemberUserId,
        Guid organizationId,
        string organizationName,
        Guid performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default)
    {
        var performerLabel = FormatPerformer(performedByDisplayName);
        var orgLabel = string.IsNullOrWhiteSpace(organizationName)
            ? "an organization"
            : organizationName.Trim();
        var message =
            $"{performerLabel} added you to “{orgLabel}”.";

        await SendToUserAsync(
            newMemberUserId,
            "added_member",
            organizationId,
            organizationName,
            message,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task NotifyMemberRemovedAsync(
        Guid removedUserId,
        Guid organizationId,
        string organizationName,
        string removedMemberDisplayName,
        Guid performedByUserId,
        string? performedByDisplayName,
        IReadOnlyList<Guid> auditRecipientUserIds,
        CancellationToken cancellationToken = default)
    {
        var performerLabel = FormatPerformer(performedByDisplayName);
        var orgLabel = string.IsNullOrWhiteSpace(organizationName)
            ? "an organization"
            : organizationName.Trim();

        var removedLine =
            $"{performerLabel} removed you from “{orgLabel}”.";

        await SendToUserAsync(
            removedUserId,
            "removed_member",
            organizationId,
            organizationName,
            removedLine,
            cancellationToken).ConfigureAwait(false);

        var memberLabel = string.IsNullOrWhiteSpace(removedMemberDisplayName)
            ? "A member"
            : removedMemberDisplayName.Trim();

        var auditLine =
            $"{performerLabel} removed {memberLabel} from “{orgLabel}”.";

        var tasks = new List<Task>();
        foreach (var adminId in auditRecipientUserIds)
        {
            if (adminId == removedUserId || adminId == performedByUserId)
                continue;

            tasks.Add(SendToUserAsync(
                adminId,
                "admin_audit_removal",
                organizationId,
                organizationName,
                auditLine,
                cancellationToken));
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static string FormatPerformer(string? performedByDisplayName) =>
        string.IsNullOrWhiteSpace(performedByDisplayName)
            ? "Someone"
            : performedByDisplayName.Trim();

    private Task SendToUserAsync(
        Guid userId,
        string audience,
        Guid organizationId,
        string organizationName,
        string message,
        CancellationToken cancellationToken)
    {
        var payload = new OrganizationMembershipNotificationDto(
            Type: "org_membership",
            Audience: audience,
            OrganizationId: organizationId,
            OrganizationName: organizationName,
            Message: message,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        cancellationToken.ThrowIfCancellationRequested();
        return hubContext.Clients.Group(NotificationHubGroups.User(userId)).NotificationReceived(payload);
    }

    private sealed record OrganizationMembershipNotificationDto(
        string Type,
        string Audience,
        Guid OrganizationId,
        string OrganizationName,
        string Message,
        DateTimeOffset CreatedAtUtc);
}
