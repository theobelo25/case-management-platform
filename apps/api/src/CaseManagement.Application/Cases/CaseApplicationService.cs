using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common;
using ServiceContracts = CaseManagement.Application.Cases.Services;

namespace CaseManagement.Application.Cases;

internal sealed class CaseApplicationService(
    ServiceContracts.ICaseCreationService creationService,
    ServiceContracts.ICaseQueryService queryService,
    ServiceContracts.ICaseWorkflowService workflowService,
    ServiceContracts.ICaseAnalyticsService analyticsService) : ICaseCommandService, ICaseQueryService, ICaseAnalyticsService
{
    public Task<CaseDetailDto> Create(
        CreateCaseInput input,
        CancellationToken cancellationToken = default) =>
        creationService.Create(input, cancellationToken);

    public Task<CaseDetailDto> GetCaseDetailAsync(
        Guid userId,
        Guid caseId,
        string? claimedOrganizationId,
        CancellationToken cancellationToken = default) =>
        queryService.GetCaseDetailAsync(userId, caseId, claimedOrganizationId, cancellationToken);

    public Task<CursorPage<CaseListItemDto>> GetCasesAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default) =>
        queryService.GetCasesAsync(input, cancellationToken);

    public Task<CursorPage<CaseListItemDto>> GetMyCasesAsync(
        GetMyCasesInput input,
        CancellationToken cancellationToken = default) =>
        queryService.GetMyCasesAsync(input, cancellationToken);

    public Task<BulkCasesResultDto> BulkUpdateCasesAsync(
        BulkCasesInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.BulkUpdateCasesAsync(input, cancellationToken);

    public Task<CaseDetailDto> UpdateCaseAsync(
        UpdateCaseInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.UpdateCaseAsync(input, cancellationToken);

    public Task<CaseDetailDto> AddCommentAsync(
        AddCaseCommentInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.AddCommentAsync(input, cancellationToken);

    public Task<CaseDetailDto> AssignCaseAsync(
        AssignCaseInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.AssignCaseAsync(input, cancellationToken);

    public Task DeleteCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.DeleteCaseAsync(input, cancellationToken);

    public Task<CaseDetailDto> ArchiveCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.ArchiveCaseAsync(input, cancellationToken);

    public Task<CaseDetailDto> UnarchiveCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default) =>
        workflowService.UnarchiveCaseAsync(input, cancellationToken);

    public Task<CaseVolumeOverTimeDto> GetCaseVolumeOverTimeAsync(
        GetCaseVolumeOverTimeInput input,
        CancellationToken cancellationToken = default) =>
        analyticsService.GetCaseVolumeOverTimeAsync(input, cancellationToken);

    public Task<FirstResponseTimeOverTimeDto> GetFirstResponseTimeOverTimeAsync(
        GetFirstResponseTimeOverTimeInput input,
        CancellationToken cancellationToken = default) =>
        analyticsService.GetFirstResponseTimeOverTimeAsync(input, cancellationToken);

    public Task<CaseStatusCountsDto> GetCaseStatusCountsAsync(
        GetCaseStatusSnapshotInput input,
        CancellationToken cancellationToken = default) =>
        analyticsService.GetCaseStatusCountsAsync(input, cancellationToken);
}
