using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal readonly record struct SlaBreachPostActionContext(
    Guid OrganizationId,
    Case CaseEntity,
    DateTimeOffset? BreachedBeforeSnapshot,
    Guid? ActorUserId,
    string? ActorDisplayName);

internal readonly record struct CaseUpdatedPostActionContext(
    Guid OrganizationId,
    Case CaseEntity,
    Guid ActorUserId,
    CaseStatus PreviousStatus,
    CasePriority PreviousPriority,
    DateTimeOffset? BreachedBeforeSnapshot);

internal readonly record struct CaseCommentedPostActionContext(
    Guid OrganizationId,
    Case CaseEntity,
    Guid AuthorUserId,
    string Body,
    bool IsInternal,
    DateTimeOffset? BreachedBeforeSnapshot);

internal readonly record struct CaseAssignedPostActionContext(
    Guid OrganizationId,
    Case CaseEntity,
    Guid ActorUserId,
    Guid? PreviousAssigneeId,
    Guid? NewAssigneeId,
    string? FromDisplayName,
    string? ToDisplayName,
    DateTimeOffset? BreachedBeforeSnapshot);

internal interface ISlaBreachPostActionHandler
{
    Task HandleAsync(SlaBreachPostActionContext context, CancellationToken cancellationToken = default);
}

internal interface ICaseUpdatedPostActionHandler
{
    Task HandleAsync(CaseUpdatedPostActionContext context, CancellationToken cancellationToken = default);
}

internal interface ICaseCommentedPostActionHandler
{
    Task HandleAsync(CaseCommentedPostActionContext context, CancellationToken cancellationToken = default);
}

internal interface ICaseAssignedPostActionHandler
{
    Task HandleAsync(CaseAssignedPostActionContext context, CancellationToken cancellationToken = default);
}

internal sealed class SlaBreachPostActionHandler(
    ICaseNotificationPublisher caseNotifications) : ISlaBreachPostActionHandler
{
    public async Task HandleAsync(
        SlaBreachPostActionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.BreachedBeforeSnapshot is not null || context.CaseEntity.SlaBreachedAtUtc is null)
            return;

        await caseNotifications.NotifySlaBreachedAsync(
            new CaseRef(context.OrganizationId, context.CaseEntity.Id, context.CaseEntity.Title),
            context.OrganizationId,
            context.CaseEntity.AssigneeUserId,
            context.ActorUserId,
            context.ActorDisplayName,
            cancellationToken);
    }
}

internal sealed class CaseUpdatedPostActionHandler(
    IUserDisplayNameLookup userDisplayNames,
    ICaseNotificationPublisher caseNotifications,
    ISlaBreachPostActionHandler slaBreachHandler) : ICaseUpdatedPostActionHandler
{
    public async Task HandleAsync(
        CaseUpdatedPostActionContext context,
        CancellationToken cancellationToken = default)
    {
        var actorName = await userDisplayNames.GetName(context.ActorUserId, cancellationToken);

        await slaBreachHandler.HandleAsync(
            new SlaBreachPostActionContext(
                context.OrganizationId,
                context.CaseEntity,
                context.BreachedBeforeSnapshot,
                context.ActorUserId,
                actorName),
            cancellationToken);

        if (context.PreviousStatus != context.CaseEntity.Status || context.PreviousPriority != context.CaseEntity.Priority)
        {
            await caseNotifications.NotifyWorkloadChangeAsync(
                new WorkloadCaseNotification(
                    new CaseRef(context.OrganizationId, context.CaseEntity.Id, context.CaseEntity.Title),
                    context.ActorUserId,
                    actorName,
                    context.PreviousStatus != context.CaseEntity.Status,
                    context.PreviousPriority != context.CaseEntity.Priority,
                    CaseServiceMappings.PriorityIncreased(context.PreviousPriority, context.CaseEntity.Priority),
                    context.PreviousStatus.ToString(),
                    context.CaseEntity.Status.ToString(),
                    context.PreviousPriority.ToString(),
                    context.CaseEntity.Priority.ToString(),
                    context.CaseEntity.AssigneeUserId,
                    context.CaseEntity.RequesterUserId),
                cancellationToken);
        }
    }
}

internal sealed class CaseCommentedPostActionHandler(
    IUserDisplayNameLookup userDisplayNames,
    ICaseNotificationPublisher caseNotifications,
    ISlaBreachPostActionHandler slaBreachHandler) : ICaseCommentedPostActionHandler
{
    public async Task HandleAsync(
        CaseCommentedPostActionContext context,
        CancellationToken cancellationToken = default)
    {
        var authorName = await userDisplayNames.GetName(context.AuthorUserId, cancellationToken);

        if (!context.IsInternal)
        {
            await caseNotifications.NotifyPublicCommentAsync(
                new CaseRef(context.OrganizationId, context.CaseEntity.Id, context.CaseEntity.Title),
                context.AuthorUserId,
                authorName,
                context.Body,
                context.CaseEntity.AssigneeUserId,
                context.CaseEntity.RequesterUserId,
                cancellationToken);
        }

        await slaBreachHandler.HandleAsync(
            new SlaBreachPostActionContext(
                context.OrganizationId,
                context.CaseEntity,
                context.BreachedBeforeSnapshot,
                context.AuthorUserId,
                authorName),
            cancellationToken);
    }
}

internal sealed class CaseAssignedPostActionHandler(
    IUserDisplayNameLookup userDisplayNames,
    ICaseNotificationPublisher caseNotifications,
    ISlaBreachPostActionHandler slaBreachHandler) : ICaseAssignedPostActionHandler
{
    public async Task HandleAsync(
        CaseAssignedPostActionContext context,
        CancellationToken cancellationToken = default)
    {
        var performerName = await userDisplayNames.GetName(context.ActorUserId, cancellationToken);

        await caseNotifications.NotifyAssignmentChangedAsync(
            [
                new CaseAssignmentChange(
                    context.OrganizationId,
                    context.CaseEntity.Id,
                    context.CaseEntity.Title,
                    context.PreviousAssigneeId,
                    context.NewAssigneeId,
                    context.FromDisplayName,
                    context.ToDisplayName),
            ],
            context.ActorUserId,
            performerName,
            cancellationToken);

        await slaBreachHandler.HandleAsync(
            new SlaBreachPostActionContext(
                context.OrganizationId,
                context.CaseEntity,
                context.BreachedBeforeSnapshot,
                context.ActorUserId,
                performerName),
            cancellationToken);
    }
}
