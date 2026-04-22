using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common;
using CaseManagement.Application.Exceptions;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal interface ICaseCreationService
{
    Task<CaseDetailDto> Create(CreateCaseInput input, CancellationToken cancellationToken = default);
}

internal interface ICaseQueryService
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

internal interface ICaseWorkflowService
{
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

internal interface ICaseAnalyticsService
{
    Task<CaseVolumeOverTimeDto> GetCaseVolumeOverTimeAsync(
        GetCaseVolumeOverTimeInput input,
        CancellationToken cancellationToken = default);

    Task<FirstResponseTimeOverTimeDto> GetFirstResponseTimeOverTimeAsync(
        GetFirstResponseTimeOverTimeInput input,
        CancellationToken cancellationToken = default);

    Task<CaseStatusCountsDto> GetCaseStatusCountsAsync(
        GetCaseStatusSnapshotInput input,
        CancellationToken cancellationToken = default);
}

internal static class CaseServiceMappings
{
    public static string ResolveSlaState(Case @case) =>
        ResolveSlaState(@case.SlaDueAtUtc, @case.SlaBreachedAtUtc, @case.SlaPausedAtUtc);

    public static string ResolveSlaState(
        DateTimeOffset? slaDueAtUtc,
        DateTimeOffset? slaBreachedAtUtc,
        DateTimeOffset? slaPausedAtUtc)
    {
        if (slaBreachedAtUtc is not null)
            return "BREACHED";

        if (slaPausedAtUtc is not null)
            return "PAUSED";

        if (slaDueAtUtc is null)
            return "NONE";

        return slaDueAtUtc <= DateTimeOffset.UtcNow
            ? "OVERDUE"
            : "ACTIVE";
    }

    public static string? ResolveAuthorDisplayName(
        Guid? userId,
        IReadOnlyDictionary<Guid, string> displayNamesByUserId)
    {
        if (userId is not { } id)
            return null;

        return displayNamesByUserId.TryGetValue(id, out var name) ? name : null;
    }

    public static string? NormalizeDisplayName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        return fullName.Trim();
    }

    public static CasePriority BumpPriority(CasePriority priority) =>
        priority switch
        {
            CasePriority.Low => CasePriority.Medium,
            CasePriority.Medium => CasePriority.High,
            CasePriority.High => CasePriority.High,
            _ => priority,
        };

    public static bool PriorityIncreased(CasePriority previous, CasePriority current) =>
        (int)previous < (int)current;
}

internal sealed class CaseAccessResolver(IUserRepository userRepository)
{
    public async Task<Guid> ResolveActiveOrganizationIdAsync(
        Guid userId,
        string? claimedOrganizationId,
        CancellationToken cancellationToken)
    {
        if (Guid.TryParse(claimedOrganizationId, out var orgId))
            return orgId;

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user?.ActiveOrganizationId is Guid activeOrgId)
            return activeOrgId;

        throw new NotFoundException("Active organization is required.", AppErrorCodes.NoActiveOrganization);
    }
}
