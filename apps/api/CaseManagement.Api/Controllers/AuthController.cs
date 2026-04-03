using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CaseManagement.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOptions<RefreshTokenOptions> _refreshOptions;

    public AuthController(IAuthService authService, IOptions<RefreshTokenOptions> refreshOptions)
    {
        _authService = authService;
        _refreshOptions = refreshOptions;
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
        var result = await _authService.SignInAsync(request, cancellationToken);
        var rt = _refreshOptions.Value;

        Response.Cookies.Append(rt.CookieName, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = result.RefreshTokenExpiresAtUtc,
            Path = rt.CookiePath
        });

        return Ok(result.Auth);
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

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Refresh)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        var rt = _refreshOptions.Value;
        var raw = Request.Cookies[rt.CookieName];

        var result = await _authService.RefreshAsync(
            raw,
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        
        Response.Cookies.Append(rt.CookieName, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = result.RefreshTokenExpiresAtUtc,
            Path = rt.CookiePath
        });

        return Ok(result.Auth);
    }

    [HttpPost("sign-out")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Refresh)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOut(
        [FromBody] SignOutRequest? body,
        CancellationToken cancellationToken)
    {
        var rt = _refreshOptions.Value;

        Guid? authenticatedUserId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = 
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (Guid.TryParse(userIdClaim, out var id))
                authenticatedUserId = id; 
        }

        await _authService.SignOutAsync(
            Request.Cookies[rt.CookieName],
            body?.RevokeAllSessions ?? false,
            authenticatedUserId,
            cancellationToken);

        Response.Cookies.Delete(
            rt.CookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = rt.CookiePath
            });

        return NoContent();
    }
}
