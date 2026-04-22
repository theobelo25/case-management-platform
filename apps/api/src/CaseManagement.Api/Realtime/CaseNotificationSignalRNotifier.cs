using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Organizations.Ports;
using Microsoft.AspNetCore.SignalR;

namespace CaseManagement.Api.Realtime;

/// <summary>
/// Production <see cref="ICaseNotificationPublisher"/> that sends case notifications to connected SignalR clients.
/// </summary>
public sealed class CaseNotificationSignalRNotifier(
    IHubContext<NotificationsHub, INotificationsClient> hubContext,
    IOrganizationPrivilegedUserIdsQuery privilegedUsers
) : ICaseNotificationPublisher
{
    public async Task NotifyAssignmentChangedAsync(
        IReadOnlyList<CaseAssignmentChange> changes,
        Guid performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default)
    {
        if (changes.Count == 0)
            return;

        var performerLabel = string.IsNullOrWhiteSpace(performedByDisplayName)
            ? "Someone"
            : performedByDisplayName.Trim();

        var tasks = new List<Task>();

        foreach (var change in changes)
        {
            if (change.FromAssigneeUserId == change.ToAssigneeUserId)
                continue;

            var title = TrimTitle(change.CaseTitle);

            if (change.ToAssigneeUserId is { } toId)
            {
                var message = $"{performerLabel} assigned you to “{title}”.";
                tasks.Add(SendAssignmentAsync(
                    toId,
                    "new_assignee",
                    change,
                    message,
                    cancellationToken));
            }

            if (change.FromAssigneeUserId is { } fromId &&
                fromId != change.ToAssigneeUserId)
            {
                string message;
                if (change.ToAssigneeUserId is null)
                {
                    message = $"{performerLabel} unassigned “{title}” from you.";
                }
                else
                {
                    var toLabel = string.IsNullOrWhiteSpace(change.ToDisplayName)
                        ? "another teammate"
                        : change.ToDisplayName.Trim();
                    message = $"{performerLabel} reassigned “{title}” from you to {toLabel}.";
                }

                tasks.Add(SendAssignmentAsync(
                    fromId,
                    "previous_assignee",
                    change,
                    message,
                    cancellationToken));
            }
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task NotifyPublicCommentAsync(
        CaseRef caseRef,
        Guid authorUserId,
        string? authorDisplayName,
        string commentPreview,
        Guid? assigneeUserId,
        Guid? requesterUserId,
        CancellationToken cancellationToken = default)
    {
        var title = TrimTitle(caseRef.CaseTitle);
        var authorLabel = string.IsNullOrWhiteSpace(authorDisplayName)
            ? "Someone"
            : authorDisplayName.Trim();
        var preview = TrimPreview(commentPreview, 160);
        var body = string.IsNullOrEmpty(preview) ? "" : $": {preview}";
        var message = $"{authorLabel} commented on “{title}”{body}";

        var tasks = new List<Task>();
        var covered = new HashSet<Guid>();

        if (assigneeUserId is { } a && a != authorUserId)
        {
            covered.Add(a);
            tasks.Add(SendCaseEventAsync(a, "public_comment", caseRef, message, cancellationToken));
        }

        if (requesterUserId is { } r &&
            r != authorUserId &&
            !covered.Contains(r))
        {
            tasks.Add(SendCaseEventAsync(r, "public_comment", caseRef, message, cancellationToken));
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task NotifySlaBreachedAsync(
        CaseRef caseRef,
        Guid organizationId,
        Guid? assigneeUserId,
        Guid? performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default)
    {
        var title = TrimTitle(caseRef.CaseTitle);
        var message = $"SLA breached on “{title}” (response-time target missed).";

        var adminIds = await privilegedUsers
            .GetOwnerAndAdminUserIdsAsync(organizationId, cancellationToken)
            .ConfigureAwait(false);

        var recipients = new HashSet<Guid>();
        if (assigneeUserId is { } asn)
            recipients.Add(asn);

        foreach (var id in adminIds)
        {
            if (performedByUserId is { } p && id == p)
                continue;
            recipients.Add(id);
        }

        var tasks = recipients
            .Select(uid => SendCaseEventAsync(uid, "sla_breached", caseRef, message, cancellationToken))
            .ToList();

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task NotifyWorkloadChangeAsync(
        WorkloadCaseNotification n,
        CancellationToken cancellationToken = default)
    {
        if (!n.StatusChanged && !n.PriorityChanged)
            return;

        if (n.PriorityChanged && !n.StatusChanged && !n.PriorityIncreased)
            return;

        string message;
        var title = TrimTitle(n.Case.CaseTitle);
        var actorLabel = string.IsNullOrWhiteSpace(n.ActorDisplayName) ? "Someone" : n.ActorDisplayName.Trim();

        if (n.StatusChanged && n.PriorityChanged)
        {
            message =
                $"{actorLabel} updated “{title}”: status {n.PreviousStatus} → {n.CurrentStatus}; priority {n.PreviousPriority} → {n.CurrentPriority}.";
        }
        else if (n.StatusChanged)
        {
            message =
                $"{actorLabel} changed status of “{title}” from {n.PreviousStatus} to {n.CurrentStatus}.";
        }
        else
        {
            message =
                $"{actorLabel} raised priority on “{title}” from {n.PreviousPriority} to {n.CurrentPriority}.";
        }

        var recipients = new HashSet<Guid>();

        if (n.AssigneeUserId is { } a && a != n.ActorUserId)
            recipients.Add(a);

        if (n.StatusChanged &&
            n.RequesterUserId is { } r &&
            r != n.ActorUserId &&
            NotifiesRequesterForStatus(n.CurrentStatus))
        {
            recipients.Add(r);
        }

        var tasks = recipients
            .Select(uid => SendCaseEventAsync(uid, "workload", n.Case, message, cancellationToken))
            .ToList();

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task NotifySlaDueSoonAsync(
        CaseRef caseRef,
        int windowMinutes,
        IReadOnlyList<Guid> recipientUserIds,
        CancellationToken cancellationToken = default)
    {
        if (recipientUserIds.Count == 0)
            return;

        var title = TrimTitle(caseRef.CaseTitle);
        var windowLabel = windowMinutes switch
        {
            >= 60 when windowMinutes % 60 == 0 => $"{windowMinutes / 60}h",
            _ => $"{windowMinutes}m"
        };

        var message = $"SLA due in {windowLabel} on “{title}”.";

        var tasks = recipientUserIds
            .Distinct()
            .Select(uid => SendCaseEventAsync(uid, "sla_due_soon", caseRef, message, cancellationToken))
            .ToList();

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static bool NotifiesRequesterForStatus(string currentStatus) =>
        currentStatus is "Pending" or "Resolved" or "Closed";

    private static string TrimTitle(string? caseTitle) =>
        string.IsNullOrWhiteSpace(caseTitle) ? "Case" : caseTitle.Trim();

    private static string TrimPreview(string body, int maxLen)
    {
        var t = body.Trim();
        if (t.Length <= maxLen)
            return t;
        return t[..(maxLen - 1)] + "…";
    }

    private Task SendAssignmentAsync(
        Guid userId,
        string audience,
        CaseAssignmentChange change,
        string message,
        CancellationToken cancellationToken)
    {
        var payload = new CaseAssignmentNotificationDto(
            Type: "case_assignment",
            Audience: audience,
            CaseId: change.CaseId,
            OrganizationId: change.OrganizationId,
            CaseTitle: change.CaseTitle,
            Message: message,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        cancellationToken.ThrowIfCancellationRequested();
        return hubContext.Clients.Group(NotificationHubGroups.User(userId)).NotificationReceived(payload);
    }

    private Task SendCaseEventAsync(
        Guid userId,
        string subtype,
        CaseRef caseRef,
        string message,
        CancellationToken cancellationToken)
    {
        var payload = new CaseEventNotificationDto(
            Type: "case_event",
            Subtype: subtype,
            CaseId: caseRef.CaseId,
            OrganizationId: caseRef.OrganizationId,
            CaseTitle: caseRef.CaseTitle,
            Message: message,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        cancellationToken.ThrowIfCancellationRequested();
        return hubContext.Clients.Group(NotificationHubGroups.User(userId)).NotificationReceived(payload);
    }

    private sealed record CaseAssignmentNotificationDto(
        string Type,
        string Audience,
        Guid CaseId,
        Guid OrganizationId,
        string CaseTitle,
        string Message,
        DateTimeOffset CreatedAtUtc);

    private sealed record CaseEventNotificationDto(
        string Type,
        string Subtype,
        Guid CaseId,
        Guid OrganizationId,
        string CaseTitle,
        string Message,
        DateTimeOffset CreatedAtUtc);
}
