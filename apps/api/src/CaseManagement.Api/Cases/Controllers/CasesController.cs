using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CasesController(
    ICasesService cases)  : ControllerBase
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
        
        var response = new CaseDetailResponse(
            created.Id,
            created.Title,
            created.Status,
            created.Priority,
            created.RequesterUserId,
            created.RequesterName,
            created.AssigneeUserId,
            created.CreatedByUserId,
            created.CreatedAtUtc,
            created.UpdatedAtUtc,
            created.Timeline
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
                .ToList()
);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}