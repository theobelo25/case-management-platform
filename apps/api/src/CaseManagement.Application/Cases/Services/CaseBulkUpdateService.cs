using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Cases.Services.BulkActions;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;

namespace CaseManagement.Application.Cases.Services;

internal sealed class CaseBulkUpdateService(
    IEnumerable<IBulkCaseActionHandler> bulkCaseActionHandlers,
    CaseAccessResolver accessResolver,
    IUserDisplayNameLookup userDisplayNames,
    ICaseRepository cases,
    IOrganizationReadRepository organizations,
    IUnitOfWork unitOfWork,
    ICaseNotificationPublisher caseNotifications) : ICaseBulkUpdateService
{
    private readonly IReadOnlyDictionary<BulkCaseAction, IBulkCaseActionHandler> _handlersByAction =
        bulkCaseActionHandlers.ToDictionary(h => h.Action);

    public async Task<BulkCasesResultDto> BulkUpdateCasesAsync(
        BulkCasesInput input,
        CancellationToken cancellationToken = default)
    {
        if (!_handlersByAction.TryGetValue(input.Action, out var handler))
            throw new ArgumentOutOfRangeException(nameof(input.Action));

        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        var distinctIds = input.CaseIds.Distinct().ToList();
        if (distinctIds.Count is < 1 or > 100)
            throw new BadRequestArgumentException("Provide between 1 and 100 case IDs.");

        var organizationEntity = await organizations.GetById(activeOrganizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");
        var slaPolicy = organizationEntity.GetSlaDurationPolicy();

        var entities = await cases.GetByIdsInOrganizationAsync(activeOrganizationId, distinctIds, cancellationToken);
        if (entities.Count != distinctIds.Count)
            throw new BadRequestArgumentException("One or more cases were not found in your organization.");

        var breachedAtStart = entities.ToDictionary(c => c.Id, c => c.SlaBreachedAtUtc);
        var statusAtStart = entities.ToDictionary(c => c.Id, c => c.Status);
        var priorityAtStart = entities.ToDictionary(c => c.Id, c => c.Priority);

        var assignmentChanges = new List<CaseAssignmentChange>();
        var nameIds = new HashSet<Guid>(
            entities.Where(c => c.AssigneeUserId is not null).Select(c => c.AssigneeUserId!.Value));

        handler.CollectNameIdsForResolution(input, nameIds);

        var names = nameIds.Count > 0
            ? await userDisplayNames.GetDisplayNamesByIdsAsync(nameIds.ToList(), cancellationToken)
            : new Dictionary<Guid, string>();

        foreach (var caseEntity in entities)
        {
            if (caseEntity.IsArchived)
                throw new BadRequestArgumentException("Archived cases cannot be updated in bulk.");

            handler.Apply(caseEntity, input, names, slaPolicy, activeOrganizationId, assignmentChanges);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var performerName = await userDisplayNames.GetName(input.UserId, cancellationToken);
        if (assignmentChanges.Count > 0)
        {
            await caseNotifications.NotifyAssignmentChangedAsync(
                assignmentChanges,
                input.UserId,
                performerName,
                cancellationToken);
        }

        foreach (var caseEntity in entities)
        {
            await TryNotifySlaBreachAsync(
                caseEntity,
                breachedAtStart[caseEntity.Id],
                activeOrganizationId,
                input.UserId,
                performerName,
                cancellationToken);

            if (!handler.TriggersWorkloadNotifications)
                continue;

            var previousStatus = statusAtStart[caseEntity.Id];
            var previousPriority = priorityAtStart[caseEntity.Id];
            if (previousStatus == caseEntity.Status && previousPriority == caseEntity.Priority)
                continue;

            await caseNotifications.NotifyWorkloadChangeAsync(
                new WorkloadCaseNotification(
                    new CaseRef(activeOrganizationId, caseEntity.Id, caseEntity.Title),
                    input.UserId,
                    performerName,
                    previousStatus != caseEntity.Status,
                    previousPriority != caseEntity.Priority,
                    CaseServiceMappings.PriorityIncreased(previousPriority, caseEntity.Priority),
                    previousStatus.ToString(),
                    caseEntity.Status.ToString(),
                    previousPriority.ToString(),
                    caseEntity.Priority.ToString(),
                    caseEntity.AssigneeUserId,
                    caseEntity.RequesterUserId),
                cancellationToken);
        }

        return new BulkCasesResultDto(entities.Count);
    }

    private async Task TryNotifySlaBreachAsync(
        Domain.Entities.Case caseEntity,
        DateTimeOffset? breachedBeforeSnapshot,
        Guid organizationId,
        Guid? actorUserId,
        string? actorDisplayName,
        CancellationToken cancellationToken)
    {
        if (breachedBeforeSnapshot is not null || caseEntity.SlaBreachedAtUtc is null)
            return;

        await caseNotifications.NotifySlaBreachedAsync(
            new CaseRef(organizationId, caseEntity.Id, caseEntity.Title),
            organizationId,
            caseEntity.AssigneeUserId,
            actorUserId,
            actorDisplayName,
            cancellationToken);
    }
}
