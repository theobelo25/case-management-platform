using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed class CaseQueryService(
    CaseAccessResolver accessResolver,
    IUserDisplayNameLookup userDisplayNames,
    ICaseRepository cases,
    ICaseListQuery caseListQuery) : ICaseQueryService
{
    public async Task<CursorPage<CaseListItemDto>> GetCasesAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default)
    {
        var casesPage = await caseListQuery.ExecuteAsync(input, cancellationToken);

        var assigneeIds = casesPage.Items
            .Select(c => c.AssigneeUserId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var assigneeNames = assigneeIds.Length > 0
            ? await userDisplayNames.GetDisplayNamesByIdsAsync(assigneeIds, cancellationToken)
            : new Dictionary<Guid, string>();

        var items = casesPage.Items
            .Select(c => new CaseListItemDto(
                Id: c.Id,
                Title: c.Title,
                Status: c.Status.ToString(),
                Priority: CaseStatusPriorityMapper.ToApiPriorityCode(c.Priority),
                SlaState: CaseServiceMappings.ResolveSlaState(c.SlaDueAtUtc, c.SlaBreachedAtUtc, c.SlaPausedAtUtc),
                SlaDueAtUtc: c.SlaDueAtUtc,
                SlaBreachedAtUtc: c.SlaBreachedAtUtc,
                SlaPausedAtUtc: c.SlaPausedAtUtc,
                SlaRemainingSeconds: c.SlaRemainingSeconds,
                RequesterUserId: c.RequesterUserId,
                RequesterName: c.RequesterName,
                AssigneeUserId: c.AssigneeUserId,
                AssigneeName: CaseServiceMappings.ResolveAuthorDisplayName(c.AssigneeUserId, assigneeNames),
                CreatedByUserId: c.CreatedByUserId,
                CreatedAtUtc: c.CreatedAtUtc,
                UpdatedAtUtc: c.UpdatedAtUtc))
            .ToArray();

        return new CursorPage<CaseListItemDto>(items, casesPage.NextCursor, casesPage.Limit);
    }

    public async Task<CursorPage<CaseListItemDto>> GetMyCasesAsync(
        GetMyCasesInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        return await GetCasesAsync(
            new GetCasesInput(
                activeOrganizationId,
                input.Cursor,
                input.Limit,
                new CaseListFilters(
                    input.Search,
                    input.Priority,
                    input.Status,
                    input.AssigneeUserId,
                    input.OverdueOnly,
                    input.BreachedOnly,
                    input.UnassignedOnly,
                    input.DueSoonWithinHours),
                new CaseListSort(input.Sort, input.SortDescending)),
            cancellationToken);
    }

    public async Task<CaseDetailDto> GetCaseDetailAsync(
        Guid userId,
        Guid caseId,
        string? claimedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            userId,
            claimedOrganizationId,
            cancellationToken);

        var caseEntity = await cases.GetById(caseId, cancellationToken)
            ?? throw new NotFoundException("Case not found.");

        if (caseEntity.OrganizationId != activeOrganizationId)
            throw new ForbiddenException("You do not have access to this case.");

        return await BuildCaseDetailDto(caseEntity, userDisplayNames, cancellationToken);
    }

    internal static async Task<CaseDetailDto> BuildCaseDetailDto(
        Case caseEntity,
        IUserDisplayNameLookup userDisplayNames,
        CancellationToken cancellationToken)
    {
        var detailAuthorIds = caseEntity.Messages
            .Select(m => m.AuthorUserId)
            .Concat(caseEntity.Events.Select(e => e.PerformedByUserId).Where(id => id != null).Cast<Guid>())
            .Append(caseEntity.CreatedByUserId)
            .Distinct()
            .ToList();

        var detailAuthorNames = await userDisplayNames.GetDisplayNamesByIdsAsync(
            detailAuthorIds,
            cancellationToken);

        var messageItems = caseEntity.Messages
            .Select(m => new CaseTimelineItemDto(
                Type: "message",
                Id: m.Id,
                CreatedAtUtc: m.CreatedAtUtc,
                AuthorUserId: m.AuthorUserId,
                AuthorDisplayName: CaseServiceMappings.ResolveAuthorDisplayName(m.AuthorUserId, detailAuthorNames),
                Body: m.Body,
                IsInternal: m.IsInternal,
                IsInitial: m.IsInitial,
                EventType: null,
                Metadata: null));

        var eventItems = caseEntity.Events
            .Select(e => new CaseTimelineItemDto(
                Type: "event",
                Id: e.Id,
                CreatedAtUtc: e.CreatedAtUtc,
                AuthorUserId: e.PerformedByUserId,
                AuthorDisplayName: CaseServiceMappings.ResolveAuthorDisplayName(e.PerformedByUserId, detailAuthorNames),
                Body: null,
                IsInternal: null,
                IsInitial: null,
                EventType: e.Type,
                Metadata: e.MetadataJson));

        var timeline = messageItems
            .Concat(eventItems)
            .OrderBy(t => t.CreatedAtUtc)
            .ToList();

        return new CaseDetailDto(
            Id: caseEntity.Id,
            OrganizationId: caseEntity.OrganizationId,
            Title: caseEntity.Title,
            Status: caseEntity.Status.ToString(),
            Priority: CaseStatusPriorityMapper.ToApiPriorityCode(caseEntity.Priority),
            SlaState: CaseServiceMappings.ResolveSlaState(caseEntity),
            IsArchived: caseEntity.IsArchived,
            SlaDueAtUtc: caseEntity.SlaDueAtUtc,
            SlaBreachedAtUtc: caseEntity.SlaBreachedAtUtc,
            SlaPausedAtUtc: caseEntity.SlaPausedAtUtc,
            SlaRemainingSeconds: caseEntity.SlaRemainingSeconds,
            RequesterUserId: caseEntity.RequesterUserId,
            RequesterName: caseEntity.RequesterName,
            AssigneeUserId: caseEntity.AssigneeUserId,
            AssigneeName: CaseServiceMappings.ResolveAuthorDisplayName(caseEntity.AssigneeUserId, detailAuthorNames),
            CreatedByUserId: caseEntity.CreatedByUserId,
            CreatedByName: CaseServiceMappings.ResolveAuthorDisplayName(caseEntity.CreatedByUserId, detailAuthorNames),
            CreatedAtUtc: caseEntity.CreatedAtUtc,
            UpdatedAtUtc: caseEntity.UpdatedAtUtc,
            Timeline: timeline);
    }
}
