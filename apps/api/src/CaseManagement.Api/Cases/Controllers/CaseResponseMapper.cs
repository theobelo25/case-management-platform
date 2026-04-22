using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Application.Cases;

namespace CaseManagement.Api.Controllers;

internal static class CaseResponseMapper
{
    public static CaseDetailResponse MapCase(CaseDetailDto dto)
    {
        return new CaseDetailResponse(
            dto.Id,
            dto.OrganizationId,
            dto.Title,
            dto.Status,
            dto.Priority,
            dto.SlaState,
            dto.IsArchived,
            dto.SlaDueAtUtc,
            dto.SlaBreachedAtUtc,
            dto.SlaPausedAtUtc,
            dto.SlaRemainingSeconds,
            dto.RequesterUserId,
            dto.RequesterName,
            dto.AssigneeUserId,
            dto.AssigneeName,
            dto.CreatedByUserId,
            dto.CreatedByName,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.Timeline
                .Select(t => new CaseTimelineItemResponse(
                    t.Type,
                    t.Id,
                    t.CreatedAtUtc,
                    t.AuthorUserId,
                    t.AuthorDisplayName,
                    t.Body,
                    t.IsInternal,
                    t.IsInitial,
                    t.EventType,
                    t.Metadata))
                .ToList());
    }

    public static CaseListItemResponse MapCaseListItem(CaseListItemDto dto)
    {
        return new CaseListItemResponse(
            dto.Id,
            dto.Title,
            dto.Status,
            dto.Priority,
            dto.SlaState,
            dto.SlaDueAtUtc,
            dto.SlaBreachedAtUtc,
            dto.SlaPausedAtUtc,
            dto.SlaRemainingSeconds,
            dto.RequesterUserId,
            dto.RequesterName,
            dto.AssigneeUserId,
            dto.AssigneeName,
            dto.CreatedByUserId,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);
    }
}
