using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Exceptions;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services.BulkActions;

internal sealed class BulkSetPriorityCaseActionHandler : IBulkCaseActionHandler
{
    public BulkCaseAction Action => BulkCaseAction.SetPriority;

    public bool TriggersWorkloadNotifications => true;

    public void CollectNameIdsForResolution(BulkCasesInput input, HashSet<Guid> nameIds)
    {
    }

    public void Apply(
        Case caseEntity,
        BulkCasesInput input,
        IReadOnlyDictionary<Guid, string> displayNamesByUserId,
        SlaDurationPolicy slaDurationPolicy,
        Guid organizationId,
        List<CaseAssignmentChange> assignmentChanges)
    {
        if (input.Priority is null)
            throw new BadRequestArgumentException("Priority is required for this action.");

        caseEntity.ChangePriority(
            CaseStatusPriorityMapper.ToDomainPriority(input.Priority.Value),
            slaDurationPolicy,
            input.UserId);
    }
}
