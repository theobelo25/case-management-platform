using CaseManagement.Application.Cases;
using CaseManagement.Application.Common;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using CaseManagement.Infrastructure.Persistence;
using CaseManagement.Infrastructure.Persistence.Queries;

namespace CaseManagement.Infrastructure.Cases.Repositories;

public sealed class CaseRepository(
    CaseManagementDbContext db,
    ICaseListQuery caseListQuery
) : ICaseRepository
{
    public void Add(
        Case @case) =>
        db.Cases.Add(@case);

    public Task<CursorPage<Case>> GetCases(
        GetCasesInput input,
        CancellationToken cancellationToken = default) =>
        caseListQuery.ExecuteAsync(input, cancellationToken);
}