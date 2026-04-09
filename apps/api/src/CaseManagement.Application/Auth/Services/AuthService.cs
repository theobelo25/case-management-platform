using System.Security.Cryptography;
using System.Text;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Auth;

public sealed class AuthService : IAuthService 
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IRefreshTokenPersistence _refreshTokenPersistence;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly TimeProvider _time;
    private readonly IUserRegistration _userRegistration;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAccessTokenIssuer accessTokenIssuer,
        IRefreshTokenPersistence refreshTokenPersistence,
        IRefreshTokenHasher refreshTokenHasher,
        TimeProvider time,
        IUserRegistration userRegistration)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _accessTokenIssuer = accessTokenIssuer;
        _refreshTokenPersistence = refreshTokenPersistence;
        _refreshTokenHasher = refreshTokenHasher;
        _time = time; 
        _userRegistration = userRegistration;
    }
    public async Task<AuthResult> RegisterAsync(
        RegisterUserInput input, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Password))
            throw new BadRequestArgumentException("Password is required.");

        var normalized = input.Email.Trim().ToLowerInvariant();

        if (await _users.GetByEmailNormalizedAsync(normalized, ct) is not null)
            throw new ConflictException("Email already registered.", code: AppErrorCodes.DuplicateEmail);
        
        var user = await _userRegistration.Register(input, ct);

        return await IssueForUserAsync(user, ct);
    }

    public async Task<AuthResult> LoginAsync(
        LoginUserInput input, 
        CancellationToken ct = default)
    {
        var normalized = input.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailNormalizedAsync(normalized, ct)
            ?? throw new AuthenticationFailedException();
        
        if (!_passwordHasher.Verify(input.Password, user.PasswordHash))
            throw new AuthenticationFailedException();
        
        return await IssueForUserAsync(user, ct);
    }

    public async Task<AuthResult> RefreshAsync(
        string refreshToken, 
        CancellationToken ct = default)
    {
        var (user, existing) = await GetActiveRefreshSessionAsync(refreshToken, ct);

        var now = _time.GetUtcNow();
        
        var revokedCount = await _refreshTokens.TryRevokeIfActiveAsync(existing.Id, now, ct);
        
        if (revokedCount != 1)
            throw new AuthenticationFailedException();
            
        return await IssueForUserAsync(user, ct);
    }

    public async Task LogoutAsync(
        string? refreshToken, 
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var (_, existing) = await GetActiveRefreshSessionAsync(refreshToken, ct);
        
        var now = _time.GetUtcNow();
        
        await _refreshTokens.TryRevokeIfActiveAsync(existing.Id, now, ct);
    }

    private async Task<AuthResult> IssueForUserAsync(User user, CancellationToken ct)
    {
        var access = _accessTokenIssuer.CreateAccessToken(
            user.Id,
            user.EmailNormalized,
            user.FirstName,
            user.LastName);
        
        var now = _time.GetUtcNow();
        var expires = now.AddDays(7);

        var persisted = await _refreshTokenPersistence.AddForUserAsync(
            user.Id,
            expires,
            now,
            ct);
    
        return new AuthResult(access, persisted.Token, persisted.ExpiresAtUtc);
    }

    private async Task<(User User, RefreshToken Token)> GetActiveRefreshSessionAsync(
        string refreshToken,
        CancellationToken ct)
    {
        if (!TrySplitRefreshToken(refreshToken, out var tokenPrefix, out _))
            throw new AuthenticationFailedException();
       
        var existing = await _refreshTokens.GetByTokenPrefixAsync(tokenPrefix, ct)
            ?? throw new AuthenticationFailedException();
        
        var hash = _refreshTokenHasher.Hash(refreshToken);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(existing.TokenHash),
                Encoding.UTF8.GetBytes(hash)))
            throw new AuthenticationFailedException();
        
        var now = _time.GetUtcNow();
        
        if (!existing.IsActive(now))
        {
            if (existing.RevokedAtUtc is not null)
            {
                await _refreshTokens.RevokeAllForUserAsync(existing.UserId, now, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            throw new AuthenticationFailedException();
        }
        
        var user = await _users.GetByIdAsync(existing.UserId, ct)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);
        
        return (user, existing);
    }

    private static bool TrySplitRefreshToken(string refreshToken, out string prefix, out string error)
    {
        prefix = string.Empty;
        error = string.Empty;

        var i = refreshToken.IndexOf(".");
        if (i <= 0 || i == refreshToken.Length - 1)
        {
            error = "Invalid token format.";
            return false;
        }

        prefix = refreshToken[..i];
        return true;
    }
}