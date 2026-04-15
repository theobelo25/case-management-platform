using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Users.Contracts;
using CaseManagement.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(UsersService users) : ControllerBase
{
    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(CursorPageResponse<UserSearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CursorPageResponse<UserSearchResponse>>> SearchAsync(
        [FromQuery] string? q,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserIdOrNull() is null)
            return Unauthorized();

        var page = await users.Search(q ?? string.Empty, cursor, limit, cancellationToken);

        var items = page.Items
            .Select(u => new UserSearchResponse(u.UserId, u.FullName, u.Email))
            .ToArray();

        return new CursorPageResponse<UserSearchResponse>(items, page.NextCursor, null, page.Limit);
    }
}
