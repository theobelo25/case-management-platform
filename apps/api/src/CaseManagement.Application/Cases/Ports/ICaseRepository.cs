using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseRepository
{
    void Add(
        Case @case);

    void Remove(
        Case @case);

    Task<Case?> GetById(
        Guid caseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Case>> GetByIdsInOrganizationAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid> caseIds,
        CancellationToken cancellationToken = default);

    Task<int> UpdateStatusAndPriorityAsync(
        Guid caseId,
        CaseStatus status,
        CasePriority priority,
        CancellationToken cancellationToken = default);
}