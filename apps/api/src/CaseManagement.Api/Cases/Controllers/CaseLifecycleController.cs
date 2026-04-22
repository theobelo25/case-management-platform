using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Ports;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CaseLifecycleController(
    ICaseCommandService caseCommandService) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCaseAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        await caseCommandService.DeleteCaseAsync(
            new CaseLifecycleCommandInput(
                context.UserId,
                id,
                context.ActiveOrganizationIdValue),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDetailResponse>> ArchiveCaseAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var updated = await caseCommandService.ArchiveCaseAsync(
            new CaseLifecycleCommandInput(
                context.UserId,
                id,
                context.ActiveOrganizationIdValue),
            cancellationToken);

        return Ok(CaseResponseMapper.MapCase(updated));
    }

    [HttpPost("{id:guid}/unarchive")]
    [ProducesResponseType(typeof(CaseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDetailResponse>> UnarchiveCaseAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var updated = await caseCommandService.UnarchiveCaseAsync(
            new CaseLifecycleCommandInput(
                context.UserId,
                id,
                context.ActiveOrganizationIdValue),
            cancellationToken);

        return Ok(CaseResponseMapper.MapCase(updated));
    }
}
