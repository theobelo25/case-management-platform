using CaseManagement.Application.Cases.Models;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services.BulkActions;

internal sealed class BulkBumpPriorityCaseActionHandler : IBulkCaseActionHandler
{
    public BulkCaseAction Action => BulkCaseAction.BumpPriority;

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
        var bumped = CaseServiceMappings.BumpPriority(caseEntity.Priority);
        caseEntity.ChangePriority(bumped, slaDurationPolicy, input.UserId);
    }
}
