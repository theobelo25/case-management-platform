using CaseManagement.Application.Cases.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagement.Application.Cases.Ports;

/// <summary>
/// Abstraction for case-related realtime notifications (assignment, replies, SLA, workload, due-soon).
/// </summary>
/// <remarks>
/// <para>
/// Implementations may deliver over SignalR, another transport, or perform no I/O at all (e.g. tests,
/// or application layers without a host). Callers must not assume that notifications are observable on the
/// client; they only invoke this port so the host can wire a real publisher when appropriate.
/// </para>
/// <para>
/// Register a production implementation from the API host via
/// <see cref="CaseManagement.Application.ApplicationRegistrationOptions.UseCaseNotificationPublisher{TService}(ServiceLifetime)"/>.
/// When that override is not used, the default registration supplies a no-op implementation.
/// </para>
/// </remarks>
public interface ICaseNotificationPublisher
{
    Task NotifyAssignmentChangedAsync(
        IReadOnlyList<CaseAssignmentChange> changes,
        Guid performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default);

    Task NotifyPublicCommentAsync(
        CaseRef caseRef,
        Guid authorUserId,
        string? authorDisplayName,
        string commentPreview,
        Guid? assigneeUserId,
        Guid? requesterUserId,
        CancellationToken cancellationToken = default);

    Task NotifySlaBreachedAsync(
        CaseRef caseRef,
        Guid organizationId,
        Guid? assigneeUserId,
        Guid? performedByUserId,
        string? performedByDisplayName,
        CancellationToken cancellationToken = default);

    Task NotifyWorkloadChangeAsync(
        WorkloadCaseNotification notification,
        CancellationToken cancellationToken = default);

    Task NotifySlaDueSoonAsync(
        CaseRef caseRef,
        int windowMinutes,
        IReadOnlyList<Guid> recipientUserIds,
        CancellationToken cancellationToken = default);
}
