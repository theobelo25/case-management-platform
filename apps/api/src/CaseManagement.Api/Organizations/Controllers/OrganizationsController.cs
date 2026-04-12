using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Organizations.Contracts;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Ports;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UserOrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserOrganizationResponse>>> ListForUserAsync(
        [FromQuery] PagingQuery query,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();
        
        var paged = await membershipsQuery.ListForUserAsync(
            userId, 
            query.Skip, 
            query.Limit, 
            cancellationToken);

        var items = paged.Items
            .Select(o => new UserOrganizationResponse(o.Id, o.Name, o.Role, o.IsArchived))
            .ToArray();
        
        return new PagedResult<UserOrganizationResponse>(
            items,
            paged.TotalCount,
            paged.Skip,
            paged.Limit,
            paged.hasMore);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(OrganizationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDetailResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();
        
        var dto = await detailQuery.GetDetailForMemberAsync(
            userId, 
            id, 
            cancellationToken);
        if (dto is null)
            return NotFound();
        
        return new OrganizationDetailResponse(
            new OrganizationResponse(
                dto.OrganizationId,
                dto.OrganizationName,
                dto.OrganizationCreatedAtUtc,
                dto.OrganizationIsArchived),
            dto.Members.Select(m => new OrganizationMemberResponse(m.UserId, m.Name, m.Role)).ToArray());
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

        var organization = await createOrganizations.CreateOrganizationAndSetOwner(
            userId,
            body.name,
            cancellationToken);

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.CreatedAtUtc,
            organization.IsArchived);
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationResponse>> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();

        var organization = await organizations.Archive(userId, id, cancellationToken);

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.CreatedAtUtc,
            organization.IsArchived);
    }

    [HttpPatch("{id:guid}/unarchive")]
    [Authorize]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationResponse>> UnarchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();

        var organization = await organizations.Unarchive(userId, id, cancellationToken);

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.CreatedAtUtc,
            organization.IsArchived);
    }

    [HttpDelete("{organizationId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        if (User.GetUserIdOrNull() is not Guid userId)
            return Unauthorized();

        await organizations.Delete(userId, organizationId, cancellationToken);

        return NoContent();
    }
}
