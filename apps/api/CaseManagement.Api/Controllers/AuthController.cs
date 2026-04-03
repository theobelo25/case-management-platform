using CaseManagement.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("sign-in")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> SignIn(
        [FromBody] SignInRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.SignInAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var response = await _authService.GetMeAsync(userId, cancellationToken);
        return Ok(response);
    }
}
