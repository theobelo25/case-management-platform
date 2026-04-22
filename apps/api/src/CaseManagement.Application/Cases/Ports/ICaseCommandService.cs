using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseCommandService
{
    Task<CaseDetailDto> Create(
        CreateCaseInput input,
        CancellationToken cancellationToken = default);

    Task<BulkCasesResultDto> BulkUpdateCasesAsync(
        BulkCasesInput input,
        CancellationToken cancellationToken = default);

    Task<CaseDetailDto> UpdateCaseAsync(
        UpdateCaseInput input,
        CancellationToken cancellationToken = default);

    Task<CaseDetailDto> AddCommentAsync(
        AddCaseCommentInput input,
        CancellationToken cancellationToken = default);

    Task<CaseDetailDto> AssignCaseAsync(
        AssignCaseInput input,
        CancellationToken cancellationToken = default);

    Task DeleteCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default);

    Task<CaseDetailDto> ArchiveCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default);

    Task<CaseDetailDto> UnarchiveCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default);
}
