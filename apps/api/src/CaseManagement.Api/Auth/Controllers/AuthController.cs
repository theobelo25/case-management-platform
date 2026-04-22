using CaseManagement.Api.Auth;
using CaseManagement.Api.Auth.Contracts;
using CaseManagement.Api.Extensions;
using CaseManagement.Api.Organizations.Contracts;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Auth.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService auth,
    IUserProfileService userProfile,
    IRefreshTokenCookieService cookieService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await auth.LoginAsync(
            new LoginUserInput(request.Email, request.Password),
            cancellationToken);

        cookieService.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        
        return Ok(new AuthResponse(result.AccessToken));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> LogoutAsync(
        CancellationToken cancellationToken = default)
    {
        cookieService.TryGetRefreshToken(Request, out var refreshToken);
        
        await auth.LogoutAsync(refreshToken, cancellationToken);
        
        cookieService.Delete(Response);

        return NoContent();
    }

    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        await userProfile.UpdateProfileAsync(
            new UpdateUserProfileInput(
                context.UserId,
                request.FirstName,
                request.LastName,
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword,
                request.ActiveOrganizationId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> GetProfileAsync(
        CancellationToken cancellationToken = default)
    {
        if (!this.TryGetUserContext(out var context))
            return Unauthorized();

        var profile = await userProfile.GetMeAsync(
            context.UserId,
            cancellationToken);

        // User-specific; must not be served from a shared or stale HTTP cache after PATCH /auth/me, etc.
        Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
        Response.Headers.Append("Pragma", "no-cache");

        return Ok(new CurrentUserResponse(
            profile.Id,
            profile.Email,
            profile.FirstName,
            profile.LastName,
            profile.ActiveOrganizationId,
            profile.Organizations
                .Select(o => new UserOrganizationResponse(o.Id, o.Name, o.Role, o.IsArchived))
                .ToArray()));
    }
}