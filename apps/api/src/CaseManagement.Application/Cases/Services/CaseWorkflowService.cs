using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed class CaseWorkflowService(
    ICaseBulkUpdateService bulkUpdater,
    CaseAccessResolver accessResolver,
    ICaseQueryService queryService,
    IUserRepository userRepository,
    ICaseRepository cases,
    IOrganizationReadRepository organizations,
    IUnitOfWork unitOfWork,
    IOrganizationCaseManagementPolicy organizationCaseManagementPolicy,
    ICaseUpdatedPostActionHandler caseUpdatedPostActionHandler,
    ICaseCommentedPostActionHandler caseCommentedPostActionHandler,
    ICaseAssignedPostActionHandler caseAssignedPostActionHandler) : ICaseWorkflowService
{
    public async Task<BulkCasesResultDto> BulkUpdateCasesAsync(
        BulkCasesInput input,
        CancellationToken cancellationToken = default)
    {
        return await bulkUpdater.BulkUpdateCasesAsync(input, cancellationToken);
    }

    public async Task<CaseDetailDto> UpdateCaseAsync(
        UpdateCaseInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        var caseEntity = await GetCaseInOrgAsync(activeOrganizationId, input.CaseId, cancellationToken);
        EnsureNotArchived(caseEntity);

        var status = CaseStatusPriorityMapper.ToDomainStatus(input.Status);
        var priority = CaseStatusPriorityMapper.ToDomainPriority(input.Priority);
        var slaPolicy = await GetSlaPolicyForOrganization(activeOrganizationId, cancellationToken);

        var prevStatus = caseEntity.Status;
        var prevPriority = caseEntity.Priority;
        var breachedBefore = caseEntity.SlaBreachedAtUtc;

        caseEntity.ChangeStatus(status, input.UserId);
        caseEntity.ChangePriority(priority, slaPolicy, input.UserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await caseUpdatedPostActionHandler.HandleAsync(
            new CaseUpdatedPostActionContext(
                activeOrganizationId,
                caseEntity,
                input.UserId,
                prevStatus,
                prevPriority,
                breachedBefore),
            cancellationToken);

        return await queryService.GetCaseDetailAsync(input.UserId, input.CaseId, input.ClaimedOrganizationId, cancellationToken);
    }

    public async Task<CaseDetailDto> AddCommentAsync(
        AddCaseCommentInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        var caseEntity = await GetCaseInOrgAsync(activeOrganizationId, input.CaseId, cancellationToken);
        EnsureNotArchived(caseEntity);

        var breachedBefore = caseEntity.SlaBreachedAtUtc;
        caseEntity.AddComment(input.UserId, input.Body, input.IsInternal);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await caseCommentedPostActionHandler.HandleAsync(
            new CaseCommentedPostActionContext(
                activeOrganizationId,
                caseEntity,
                input.UserId,
                input.Body,
                input.IsInternal,
                breachedBefore),
            cancellationToken);
        return await queryService.GetCaseDetailAsync(input.UserId, input.CaseId, input.ClaimedOrganizationId, cancellationToken);
    }

    public async Task<CaseDetailDto> AssignCaseAsync(
        AssignCaseInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        var caseEntity = await GetCaseInOrgAsync(activeOrganizationId, input.CaseId, cancellationToken);
        EnsureNotArchived(caseEntity);

        var breachedBefore = caseEntity.SlaBreachedAtUtc;
        var previousAssigneeId = caseEntity.AssigneeUserId;
        var fromDisplayName = await ResolveAssigneeNameAsync(caseEntity.AssigneeUserId, cancellationToken);

        string? toDisplayName = null;
        if (input.AssigneeUserId is Guid assigneeUserId)
        {
            toDisplayName = await ResolveAssigneeNameAsync(assigneeUserId, cancellationToken);
            caseEntity.AssignTo(assigneeUserId, input.UserId, fromDisplayName, toDisplayName);
        }
        else
        {
            caseEntity.Unassign(input.UserId, fromDisplayName);
        }

        var newAssigneeId = caseEntity.AssigneeUserId;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await caseAssignedPostActionHandler.HandleAsync(
            new CaseAssignedPostActionContext(
                activeOrganizationId,
                caseEntity,
                input.UserId,
                previousAssigneeId,
                newAssigneeId,
                fromDisplayName,
                toDisplayName,
                breachedBefore),
            cancellationToken);
        return await queryService.GetCaseDetailAsync(input.UserId, input.CaseId, input.ClaimedOrganizationId, cancellationToken);
    }

    public async Task DeleteCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);
        await organizationCaseManagementPolicy.EnsureUserCanManageCases(input.UserId, activeOrganizationId, cancellationToken);

        var caseEntity = await GetCaseInOrgAsync(activeOrganizationId, input.CaseId, cancellationToken);
        cases.Remove(caseEntity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CaseDetailDto> ArchiveCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);
        await organizationCaseManagementPolicy.EnsureUserCanManageCases(input.UserId, activeOrganizationId, cancellationToken);

        var caseEntity = await GetCaseInOrgAsync(activeOrganizationId, input.CaseId, cancellationToken);
        caseEntity.Archive(input.UserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await queryService.GetCaseDetailAsync(input.UserId, input.CaseId, input.ClaimedOrganizationId, cancellationToken);
    }

    public async Task<CaseDetailDto> UnarchiveCaseAsync(
        CaseLifecycleCommandInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);
        await organizationCaseManagementPolicy.EnsureUserCanManageCases(input.UserId, activeOrganizationId, cancellationToken);

        var caseEntity = await GetCaseInOrgAsync(activeOrganizationId, input.CaseId, cancellationToken);
        caseEntity.Unarchive(input.UserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await queryService.GetCaseDetailAsync(input.UserId, input.CaseId, input.ClaimedOrganizationId, cancellationToken);
    }

    private async Task<Case> GetCaseInOrgAsync(Guid organizationId, Guid caseId, CancellationToken cancellationToken)
    {
        var caseEntity = await cases.GetById(caseId, cancellationToken)
            ?? throw new NotFoundException("Case not found.");

        if (caseEntity.OrganizationId != organizationId)
            throw new ForbiddenException("You do not have access to this case.");

        return caseEntity;
    }

    private void EnsureNotArchived(Case caseEntity)
    {
        if (caseEntity.IsArchived)
            throw new BadRequestArgumentException("This case is archived.");
    }

    private async Task<SlaDurationPolicy> GetSlaPolicyForOrganization(Guid organizationId, CancellationToken cancellationToken)
    {
        var organizationEntity = await organizations.GetById(organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");
        return organizationEntity.GetSlaDurationPolicy();
    }

    private async Task<string?> ResolveAssigneeNameAsync(Guid? assigneeUserId, CancellationToken cancellationToken)
    {
        if (assigneeUserId is null)
            return null;

        var user = await userRepository.GetByIdAsync(assigneeUserId.Value, cancellationToken);
        return user?.FullName;
    }

}
