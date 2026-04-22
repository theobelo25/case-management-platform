using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CaseQueriesController(
    ICaseQueryService caseQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CursorPageResponse<CaseListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CursorPageResponse<CaseListItemResponse>>> GetCasesAsync(
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null,
        [FromQuery] string? search = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? status = null,
        [FromQuery] bool overdueOnly = false,
        [FromQuery] bool breachedOnly = false,
        [FromQuery] bool unassignedOnly = false,
        [FromQuery] int? dueSoonWithinHours = null,
        [FromQuery] string? sort = null,
        [FromQuery] bool? sortDescending = null,
        [FromQuery] bool assignedToMe = false,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!CasesApiValidator.TryValidateCaseListQuery(
                ModelState,
                limit,
                assignedToMe,
                unassignedOnly,
                dueSoonWithinHours,
                sort,
                out var normalizedSort))
        {
            return ValidationProblem(ModelState);
        }

        var page = await caseQueryService.GetMyCasesAsync(
            new GetMyCasesInput(
                context.UserId,
                context.ActiveOrganizationIdValue,
                cursor,
                limit,
                search,
                priority,
                status,
                normalizedSort,
                sortDescending ?? false,
                assignedToMe ? context.UserId : null,
                overdueOnly,
                breachedOnly,
                unassignedOnly,
                dueSoonWithinHours),
            cancellationToken);

        return Ok(new CursorPageResponse<CaseListItemResponse>(
            page.Items.Select(CaseResponseMapper.MapCaseListItem).ToList(),
            page.NextCursor,
            null,
            page.Limit));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CaseDetailResponse>> GetCaseDetail(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var detail = await caseQueryService.GetCaseDetailAsync(
            context.UserId,
            id,
            context.ActiveOrganizationIdValue,
            cancellationToken);

        return Ok(CaseResponseMapper.MapCase(detail));
    }
}
