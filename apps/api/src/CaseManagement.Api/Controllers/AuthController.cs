using CaseManagement.Api.Auth;
using CaseManagement.Api.Contracts;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService auth,
    IRefreshTokenCookieService cookieService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(
            new RegisterUserInput(
                request.Email,
                request.Password,
                request.FirstNameForValidation,
                request.LastNameForValidation),
            cancellationToken);
        
        cookieService.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return Ok(new AuthResponse(result.AccessToken));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(request.Email, request.Password, cancellationToken);

        cookieService.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        
        return Ok(new AuthResponse(result.AccessToken));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> RefreshAsync(CancellationToken cancellationToken)
    {
        if (!cookieService.TryGetRefreshToken(Request, out var refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AuthenticationFailedException("Refresh token missing or invalid.");
        }

        var result = await auth.RefreshAsync(refreshToken, cancellationToken);
        
        cookieService.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return Ok(new AuthResponse(result.AccessToken));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        cookieService.TryGetRefreshToken(Request, out var refreshToken);
        
        await auth.LogoutAsync(refreshToken, cancellationToken);
        
        cookieService.Delete(Response);

        return NoContent();
    }
}