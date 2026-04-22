using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CaseCreationController(
    ICaseCommandService caseCommandService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("cases")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseDetailResponse>> CreateAsync(
        [FromBody] CreateCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var input = new CreateCaseInput(
            request.Title,
            request.InitialMessage,
            request.Priority,
            request.RequesterUserId,
            context.UserId);

        var created = await caseCommandService.Create(input, cancellationToken);
        var response = CaseResponseMapper.MapCase(created);
        return Created($"/api/cases/{response.Id}", response);
    }

    [HttpPost("bulk")]
    [EnableRateLimiting("cases")]
    [ProducesResponseType(typeof(BulkCasesResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BulkCasesResultResponse>> BulkCasesAsync(
        [FromBody] BulkCasesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!CasesApiValidator.TryValidateBulkRequest(
                ModelState,
                request,
                out var action,
                out var priorityCode,
                out var statusCode))
        {
            return ValidationProblem(ModelState);
        }

        var result = await caseCommandService.BulkUpdateCasesAsync(
            new BulkCasesInput(
                context.UserId,
                context.ActiveOrganizationIdValue,
                request.CaseIds,
                action,
                request.AssigneeUserId,
                priorityCode,
                statusCode),
            cancellationToken);

        return Ok(new BulkCasesResultResponse(result.UpdatedCount));
    }
}
