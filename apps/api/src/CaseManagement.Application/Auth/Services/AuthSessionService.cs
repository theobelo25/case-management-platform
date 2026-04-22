using System.Security.Cryptography;
using System.Text;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Exceptions;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Auth;

internal sealed class AuthSessionService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenPersistence refreshTokenPersistence,
    IRefreshTokenHasher refreshTokenHasher,
    TimeProvider timeProvider) : IAuthSessionService
{
    public async Task<AuthResult> IssueForUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var access = accessTokenIssuer.CreateAccessToken(
            user.Id,
            user.EmailNormalized,
            user.FirstName,
            user.LastName);

        var now = timeProvider.GetUtcNow();
        var expires = now.AddDays(7);

        var persisted = await refreshTokenPersistence.AddForUserAsync(
            user.Id,
            expires,
            now,
            cancellationToken);

        return new AuthResult(access, persisted.Token, persisted.ExpiresAtUtc);
    }

    public async Task<(User User, RefreshToken Token)> GetActiveRefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (!TrySplitRefreshToken(refreshToken, out var tokenPrefix))
            throw new AuthenticationFailedException();

        var existing = await refreshTokens.GetByTokenPrefixAsync(tokenPrefix, cancellationToken)
            ?? throw new AuthenticationFailedException();

        var hash = refreshTokenHasher.Hash(refreshToken);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(existing.TokenHash),
                Encoding.UTF8.GetBytes(hash)))
        {
            throw new AuthenticationFailedException();
        }

        var now = timeProvider.GetUtcNow();
        if (!existing.IsActive(now))
        {
            if (existing.RevokedAtUtc is not null)
            {
                await refreshTokens.RevokeAllForUserAsync(existing.UserId, now, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            throw new AuthenticationFailedException();
        }

        var user = await users.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);

        return (user, existing);
    }

    private static bool TrySplitRefreshToken(string refreshToken, out string prefix)
    {
        prefix = string.Empty;
        var delimiterIndex = refreshToken.IndexOf(".");
        if (delimiterIndex <= 0 || delimiterIndex == refreshToken.Length - 1)
            return false;

        prefix = refreshToken[..delimiterIndex];
        return true;
    }
}
