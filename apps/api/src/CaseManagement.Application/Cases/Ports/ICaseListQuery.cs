using CaseManagement.Application.Cases;
using CaseManagement.Application.Common;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface ICaseListQuery
{
    Task<CursorPage<Case>> ExecuteAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default);
}