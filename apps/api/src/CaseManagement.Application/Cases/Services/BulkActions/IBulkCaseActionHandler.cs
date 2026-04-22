using CaseManagement.Application.Cases.Models;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services.BulkActions;

internal interface IBulkCaseActionHandler
{
    BulkCaseAction Action { get; }

    /// <summary>
    /// When true, the orchestrator emits workload notifications when status or priority changes.
    /// </summary>
    bool TriggersWorkloadNotifications { get; }

    void CollectNameIdsForResolution(BulkCasesInput input, HashSet<Guid> nameIds);

    void Apply(
        Case caseEntity,
        BulkCasesInput input,
        IReadOnlyDictionary<Guid, string> displayNamesByUserId,
        SlaDurationPolicy slaDurationPolicy,
        Guid organizationId,
        List<CaseAssignmentChange> assignmentChanges);
}
