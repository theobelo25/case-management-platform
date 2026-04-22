using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Api.Organizations.Contracts;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Organizations.Ports;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/organizations")]
public sealed class OrganizationsController(
    IOrganizationsService organizations,
    ICreateOrganizationService createOrganizations,
    IUserOrganizationMembershipsQuery membershipsQuery,
    IOrganizationDetailQuery detailQuery
) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserOrganizationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<UserOrganizationResponse>>> ListForUserAsync(
        [FromQuery] PagingQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!OrganizationsControllerValidator.TryValidatePagingQuery(ModelState, query))
            return ValidationProblem(ModelState);
        
        var paged = await membershipsQuery.ListForUserAsync(
            context.UserId, 
            query.Skip, 
            query.Limit, 
            cancellationToken);

        var items = paged.Items
            .Select(OrganizationResponseMapper.MapUserOrganization)
            .ToArray();
        
        return new PagedResult<UserOrganizationResponse>(
            items,
            paged.TotalCount,
            paged.Skip,
            paged.Limit,
            paged.HasMore);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDetailResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();
        
        var dto = await detailQuery.GetDetailForMemberAsync(
            context.UserId, 
            id, 
            cancellationToken);
        if (dto is null)
            return NotFound();
        
        return OrganizationResponseMapper.MapOrganizationDetail(dto);
    }

    [HttpPut("{id:guid}/sla-policy")]
    [ProducesResponseType(typeof(OrganizationSlaPolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationSlaPolicyResponse>> UpdateSlaPolicyAsync(
        Guid id,
        [FromBody] UpdateOrganizationSlaPolicyRequest body,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!OrganizationsControllerValidator.TryValidateSlaPolicyRequest(ModelState, body))
            return ValidationProblem(ModelState);

        var updated = await organizations.UpdateSlaPolicy(
            context.UserId,
            id,
            body.LowHours,
            body.MediumHours,
            body.HighHours,
            cancellationToken);

        return OrganizationResponseMapper.MapSlaPolicy(updated);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrganizationResponse>> CreateAsync(
        [FromBody] CreateOrganizationRequest body,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!OrganizationsControllerValidator.TryValidateCreateRequest(ModelState, body))
            return ValidationProblem(ModelState);

        var organization = await createOrganizations.CreateOrganizationAndSetOwner(
            context.UserId,
            body.Name,
            cancellationToken);

        var response = OrganizationResponseMapper.MapOrganization(organization);

        return Created($"/api/organizations/{response.Id}", response);
    }

    [HttpPatch("{id:guid}/archive")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationResponse>> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var organization = await organizations.Archive(context.UserId, id, cancellationToken);

        return OrganizationResponseMapper.MapOrganization(organization);
    }

    [HttpPatch("{id:guid}/unarchive")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationResponse>> UnarchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var organization = await organizations.Unarchive(context.UserId, id, cancellationToken);

        return OrganizationResponseMapper.MapOrganization(organization);
    }

    [HttpDelete("{organizationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        await organizations.Delete(context.UserId, organizationId, cancellationToken);

        return NoContent();
    }

    [HttpPost("{organizationId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(
        Guid organizationId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        await organizations.AddMember(
            context.UserId, 
            memberId, 
            organizationId, 
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{organizationId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        Guid organizationId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        await organizations.RemoveMember(
            context.UserId, 
            memberId, 
            organizationId, 
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{organizationId:guid}/transfer-ownership")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferOwnershipAsync(
        Guid organizationId,
        [FromBody] TransferOwnershipRequest body,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        if (!OrganizationsControllerValidator.TryValidateTransferOwnershipRequest(ModelState, body))
            return ValidationProblem(ModelState);

        await organizations.TransferOwnership(
            context.UserId,
            body.NewOwnerUserId,
            organizationId,
            cancellationToken);

        return NoContent();
    }
}
