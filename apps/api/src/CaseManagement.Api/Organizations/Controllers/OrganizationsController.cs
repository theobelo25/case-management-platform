using CaseManagement.Api.Organizations.Contracts;
using CaseManagement.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/organizations")]
public sealed class OrganizationsController(
    ICreateOrganizationService organizations
) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<OrganizationResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _ = id;
        _ = cancellationToken;
        return Task.FromResult<ActionResult<OrganizationResponse>>(NotFound());
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationResponse>> CreateAsync(
        [FromBody] CreateOrganizationRequest body,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();

        var organization = await organizations.CreateOrganizationAndSetOwner(
            userId,
            body.name,
            cancellationToken);

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.CreatedAtUtc
        );
    }
}
