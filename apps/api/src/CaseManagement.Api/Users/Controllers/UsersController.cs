using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Api.Users.Contracts;
using CaseManagement.Application.Users;
using CaseManagement.Application.Users.Ports;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IUsersService users) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(CursorPageResponse<UserSearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CursorPageResponse<UserSearchResponse>>> SearchAsync(
        [FromQuery] string? q,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var page = await users.SearchForRequesterAsync(
            new SearchUsersInput(context.UserId, q, cursor, limit),
            cancellationToken);

        var items = page.Items
            .Select(u => new UserSearchResponse(u.UserId, u.FullName, u.Email))
            .ToArray();

        return new CursorPageResponse<UserSearchResponse>(items, page.NextCursor, null, page.Limit);
    }
}
