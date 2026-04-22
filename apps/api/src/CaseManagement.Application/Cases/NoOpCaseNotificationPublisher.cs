using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagement.Application.Cases;

/// <summary>
/// Completes all notification methods without side effects. Used as the default when
/// <see cref="CaseManagement.Application.ApplicationRegistrationOptions.UseCaseNotificationPublisher{TService}(ServiceLifetime)"/> is not called
/// (e.g. tests or hosts that do not push realtime updates).
/// </summary>
internal sealed class NoOpCaseNotificationPublisher : ICaseNotificationPublisher
{
    public Task NotifyAssignmentChangedAsync(
        IReadOnlyList<CaseAssignmentChange> changes,
        Guid performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifyPublicCommentAsync(
        CaseRef caseRef,
        Guid authorUserId,
        string? authorDisplayName,
        string commentPreview,
        Guid? assigneeUserId,
        Guid? requesterUserId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifySlaBreachedAsync(
        CaseRef caseRef,
        Guid organizationId,
        Guid? assigneeUserId,
        Guid? performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifyWorkloadChangeAsync(
        WorkloadCaseNotification notification,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifySlaDueSoonAsync(
        CaseRef caseRef,
        int windowMinutes,
        IReadOnlyList<Guid> recipientUserIds,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
