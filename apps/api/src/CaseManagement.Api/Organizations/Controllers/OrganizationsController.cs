using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Organizations.Contracts;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/organizations")]
public sealed class OrganizationsController(
    ICreateOrganizationService organizations,
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
            .Select(o => new UserOrganizationResponse(o.Id, o.Name, o.Role))
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
                dto.OrganizationCreatedAtUtc
            ),
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
