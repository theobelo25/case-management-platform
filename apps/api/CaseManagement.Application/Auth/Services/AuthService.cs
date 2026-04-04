using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common.Exceptions;
using CaseManagement.Domain.Users;

namespace CaseManagement.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenIssuer _refreshTokenIssuer;
    private readonly IRefreshTokenValidator _refreshTokenValidator;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenIssuer refreshTokenIssuer,
        IRefreshTokenValidator refreshTokenValidator,
        IRefreshTokenStore refreshTokenStore)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenIssuer = refreshTokenIssuer;
        _refreshTokenValidator = refreshTokenValidator;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<SignInResult> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        var token = _jwtTokenService.CreateAccessToken(
            user.Id,
            user.Email,
            user.FullName);

        var (refreshRaw, refreshExpires) =
            await _refreshTokenIssuer.IssueAsync(user.Id, cancellationToken);

        return new SignInResult(
            new AuthResponse(
                token.AccessToken,
                token.ExpiresAtUtc,
                user.Id,
                user.Email,
                user.FullName),
            refreshRaw,
            refreshExpires);
    }

    public async Task<SignInResult> SignUpAsync(
        SignUpRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _users.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
            throw new ConflictException("An account with this email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(
            Guid.NewGuid(),
            normalizedEmail,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            passwordHash,
            DateTime.UtcNow);

        await _users.AddAsync(user, cancellationToken);

        var token = _jwtTokenService.CreateAccessToken(
            user.Id,
            user.Email,
            user.FullName);

        var (refreshRaw, refreshExpires) =
            await _refreshTokenIssuer.IssueAsync(user.Id, cancellationToken);

        return new SignInResult(
            new AuthResponse(
                token.AccessToken,
                token.ExpiresAtUtc,
                user.Id,
                user.Email,
                user.FullName),
            refreshRaw,
            refreshExpires);
    }

    public async Task<MeResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new MeResponse(user.Id, user.Email, user.FullName);
    }

    public async Task<SignInResult> RefreshAsync(
        string? refreshTokenRaw,
        string? clientUserAgent,
        string? clientIpAddress,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var session = await _refreshTokenValidator.ValidateAsync(
            refreshTokenRaw ?? string.Empty,
            utcNow,
            cancellationToken);

        if (session is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await _users.GetByIdAsync(session.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        var token = _jwtTokenService.CreateAccessToken(user.Id, user.Email, user.FullName);

        var (refreshRaw, refreshExpires) = await _refreshTokenIssuer.RotateAsync(
            session,
            clientUserAgent,
            clientIpAddress,
            cancellationToken);

        return new SignInResult(
            new AuthResponse(
                token.AccessToken,
                token.ExpiresAtUtc,
                user.Id,
                user.Email,
                user.FullName),
            refreshRaw,
            refreshExpires);
    }

    public async Task SignOutAsync(
        string? refreshTokenRaw,
        bool revokeAllSessions,
        Guid? authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        RefreshTokenSession? session = null;
        if (!string.IsNullOrWhiteSpace(refreshTokenRaw))
        {
            session = await _refreshTokenValidator.ValidateAsync(
                refreshTokenRaw,
                utcNow,
                cancellationToken);
        }
        var userIdForRevokeAll = authenticatedUserId ?? session?.UserId;
        if (revokeAllSessions && userIdForRevokeAll is Guid uid)
        {
            await _refreshTokenStore.RevokeAllForUserAsync(uid, cancellationToken);
            return;
        }
        if (session is not null)
            await _refreshTokenStore.RevokeAsync(session.Id, cancellationToken);
    }
}
