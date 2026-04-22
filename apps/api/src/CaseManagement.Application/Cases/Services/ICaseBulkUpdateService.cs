using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Services;

internal interface ICaseBulkUpdateService
{
    Task<BulkCasesResultDto> BulkUpdateCasesAsync(
        BulkCasesInput input,
        CancellationToken cancellationToken = default);
}
