using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Exceptions;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services.BulkActions;

internal sealed class BulkAssignCaseActionHandler : IBulkCaseActionHandler
{
    public BulkCaseAction Action => BulkCaseAction.Assign;

    public bool TriggersWorkloadNotifications => false;

    public void CollectNameIdsForResolution(BulkCasesInput input, HashSet<Guid> nameIds)
    {
        if (input.AssigneeUserId is { } assignTo)
            nameIds.Add(assignTo);
    }

    public void Apply(
        Case caseEntity,
        BulkCasesInput input,
        IReadOnlyDictionary<Guid, string> displayNamesByUserId,
        SlaDurationPolicy slaDurationPolicy,
        Guid organizationId,
        List<CaseAssignmentChange> assignmentChanges)
    {
        if (caseEntity.Status == CaseStatus.Closed)
            throw new BadRequestArgumentException("Closed cases cannot be reassigned.");

        var fromName = CaseServiceMappings.ResolveAuthorDisplayName(caseEntity.AssigneeUserId, displayNamesByUserId);
        var previousAssigneeId = caseEntity.AssigneeUserId;

        if (input.AssigneeUserId is Guid assigneeId)
        {
            var toName = CaseServiceMappings.ResolveAuthorDisplayName(assigneeId, displayNamesByUserId);
            caseEntity.AssignTo(assigneeId, input.UserId, fromName, toName);
            assignmentChanges.Add(new CaseAssignmentChange(
                organizationId,
                caseEntity.Id,
                caseEntity.Title,
                previousAssigneeId,
                caseEntity.AssigneeUserId,
                fromName,
                toName));
        }
        else
        {
            caseEntity.Unassign(input.UserId, fromName);
            assignmentChanges.Add(new CaseAssignmentChange(
                organizationId,
                caseEntity.Id,
                caseEntity.Title,
                previousAssigneeId,
                null,
                fromName,
                null));
        }
    }
}
