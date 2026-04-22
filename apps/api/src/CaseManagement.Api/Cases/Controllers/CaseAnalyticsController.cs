using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CaseAnalyticsController(
    ICaseAnalyticsService caseAnalyticsService) : ControllerBase
{
    [HttpGet("volume-over-time")]
    [ProducesResponseType(typeof(CaseVolumeOverTimeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseVolumeOverTimeResponse>> GetCaseVolumeOverTimeAsync(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!CasesApiValidator.TryValidateDays(ModelState, nameof(days), days))
            return ValidationProblem(ModelState);

        var dto = await caseAnalyticsService.GetCaseVolumeOverTimeAsync(
            new GetCaseVolumeOverTimeInput(
                context.UserId,
                context.ActiveOrganizationIdValue,
                days),
            cancellationToken);

        return Ok(new CaseVolumeOverTimeResponse(
            dto.Series
                .Select(p => new CaseVolumeDayPointResponse(
                    p.Date,
                    p.CasesCreated,
                    p.CasesResolved,
                    p.CasesReopened))
                .ToList()));
    }

    [HttpGet("first-response-time-over-time")]
    [ProducesResponseType(typeof(FirstResponseTimeOverTimeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FirstResponseTimeOverTimeResponse>> GetFirstResponseTimeOverTimeAsync(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!CasesApiValidator.TryValidateDays(ModelState, nameof(days), days))
            return ValidationProblem(ModelState);

        var dto = await caseAnalyticsService.GetFirstResponseTimeOverTimeAsync(
            new GetFirstResponseTimeOverTimeInput(
                context.UserId,
                context.ActiveOrganizationIdValue,
                days),
            cancellationToken);

        return Ok(new FirstResponseTimeOverTimeResponse(
            dto.Series
                .Select(p => new FirstResponseTimeDayPointResponse(
                    p.Date,
                    p.AverageFirstResponseMinutes,
                    p.CasesWithFirstResponse))
                .ToList()));
    }

    [HttpGet("count-by-status")]
    [ProducesResponseType(typeof(CaseStatusCountsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseStatusCountsResponse>> GetCaseStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var dto = await caseAnalyticsService.GetCaseStatusCountsAsync(
            new GetCaseStatusSnapshotInput(
                context.UserId,
                context.ActiveOrganizationIdValue),
            cancellationToken);

        return Ok(new CaseStatusCountsResponse(
            dto.NewCount,
            dto.OpenCount,
            dto.PendingCount,
            dto.ResolvedCount,
            dto.ClosedCount));
    }
}
