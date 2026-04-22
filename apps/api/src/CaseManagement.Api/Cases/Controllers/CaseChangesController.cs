using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CaseChangesController(
    ICaseCommandService caseCommandService) : ControllerBase
{
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CaseDetailResponse>> UpdateCaseAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!CasesApiValidator.TryValidateUpdateRequest(
                ModelState,
                request,
                out var statusCode,
                out var priorityCode))
        {
            return ValidationProblem(ModelState);
        }

        var updated = await caseCommandService.UpdateCaseAsync(
            new UpdateCaseInput(
                context.UserId,
                id,
                context.ActiveOrganizationIdValue,
                statusCode,
                priorityCode),
            cancellationToken);

        return Ok(CaseResponseMapper.MapCase(updated));
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CaseDetailResponse>> AddCommentAsync(
        [FromRoute] Guid id,
        [FromBody] AddCaseCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var updated = await caseCommandService.AddCommentAsync(
            new AddCaseCommentInput(
                context.UserId,
                id,
                context.ActiveOrganizationIdValue,
                request.Body,
                request.IsInternal),
            cancellationToken);

        return Ok(CaseResponseMapper.MapCase(updated));
    }

    [HttpPatch("{id:guid}/assignee")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CaseDetailResponse>> AssignCaseAsync(
        [FromRoute] Guid id,
        [FromBody] AssignCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var updated = await caseCommandService.AssignCaseAsync(
            new AssignCaseInput(
                context.UserId,
                id,
                context.ActiveOrganizationIdValue,
                request.AssigneeUserId),
            cancellationToken);

        return Ok(CaseResponseMapper.MapCase(updated));
    }
}
