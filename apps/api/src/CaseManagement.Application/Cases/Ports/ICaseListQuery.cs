using CaseManagement.Application.Cases;
using CaseManagement.Application.Common;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseListQuery
{
    Task<CursorPage<CaseListItemReadModel>> ExecuteAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default);
}