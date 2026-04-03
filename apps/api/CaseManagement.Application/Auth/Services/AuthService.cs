using CaseManagement.Application.Common.Exceptions;

namespace CaseManagement.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> SignInAsync(
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

        return new AuthResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            user.Id,
            user.Email,
            user.FullName);
    }
    public async Task<MeResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new MeResponse(user.Id, user.Email, user.FullName);
    }
}
