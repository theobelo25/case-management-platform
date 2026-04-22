using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Exceptions;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services.BulkActions;

internal sealed class BulkSetStatusCaseActionHandler : IBulkCaseActionHandler
{
    public BulkCaseAction Action => BulkCaseAction.SetStatus;

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
        if (input.Status is null)
            throw new BadRequestArgumentException("Status is required for this action.");

        caseEntity.ChangeStatus(CaseStatusPriorityMapper.ToDomainStatus(input.Status.Value), input.UserId);
    }
}
