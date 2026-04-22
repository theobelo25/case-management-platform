using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Common;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseQueryService
{
    Task<CaseDetailDto> GetCaseDetailAsync(
        Guid userId,
        Guid caseId,
        string? claimedOrganizationId,
        CancellationToken cancellationToken = default);

    Task<CursorPage<CaseListItemDto>> GetCasesAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default);

    Task<CursorPage<CaseListItemDto>> GetMyCasesAsync(
        GetMyCasesInput input,
        CancellationToken cancellationToken = default);
}
