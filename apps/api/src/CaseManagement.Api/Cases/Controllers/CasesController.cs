using System.Runtime.InteropServices;
using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Api.Common.Contracts;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CasesController(
    ICasesService cases,
    IUserRepository users)  : ControllerBase
{
    [HttpPost]
    [Authorize]
    [EnableRateLimiting("cases")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseDetailResponse>> CreateAsync(
        [FromBody] CreateCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();

        var input = new CreateCaseInput(
            request.Title,
            request.InitialMessage,
            request.Priority,
            request.RequesterUserId,
            userId);
        
        var created = await cases.Create(
            input, 
            cancellationToken);
        
        var response = MapCase(created);
        
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<CursorPageResponse<CaseDetailResponse>>> GetCasesAsync(
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null,
        [FromQuery] string? search = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sort = null,
        [FromQuery] bool? sortDescending = null,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();

        var organizationId = await ResolveActiveOrganizationIdAsync(userId, cancellationToken);
        if (organizationId is not Guid activeOrganizationId)
            return BadRequest("Active organization is required.");
        
        var page = await cases.GetCasesAsync(
            new GetCasesInput(
                activeOrganizationId,
                cursor,
                limit,
                new CaseListFilters(search, priority, status),
                new CaseListSort(sort, sortDescending ?? false)),
            cancellationToken);

        var response = new CursorPageResponse<CaseListItemResponse>(
            page.Items.Select(MapCaseListItem).ToList(),
            page.NextCursor,
            null,
            page.Limit);

        return Ok(response);
    }

    private async Task<Guid?> ResolveActiveOrganizationIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var claimedOrganizationId = User.GetActiveOrganizationIdOrNull();
        if (claimedOrganizationId is Guid organizationId)
            return organizationId;

        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user?.ActiveOrganizationId;
    }

    private static CaseDetailResponse MapCase(CaseDetailDto dto)
    {
        return new CaseDetailResponse(
            dto.Id,
            dto.Title,
            dto.Status,
            dto.Priority,
            dto.RequesterUserId,
            dto.RequesterName,
            dto.AssigneeUserId,
            dto.CreatedByUserId,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.Timeline
                .Select(t => new CaseTimelineItemResponse(
                    t.Type,
                    t.Id,
                    t.CreatedAtUtc,
                    t.AuthorUserId,
                    t.Body,
                    t.IsInternal,
                    t.IsInitial,
                    t.EventType,
                    t.Metadata))
                .ToList());
    }

    private static CaseListItemResponse MapCaseListItem(CaseListItemDto dto)
    {
        return new CaseListItemResponse(
            dto.Id,
            dto.Title,
            dto.Status,
            dto.Priority,
            dto.RequesterUserId,
            dto.RequesterName,
            dto.AssigneeUserId,
            dto.CreatedByUserId,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);
    }
}