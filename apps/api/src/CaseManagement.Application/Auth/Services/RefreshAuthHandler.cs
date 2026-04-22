using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Exceptions;

namespace CaseManagement.Application.Auth;

internal sealed class RefreshAuthHandler(
    IRefreshTokenRepository refreshTokens,
    IAuthSessionService authSessionService,
    TimeProvider timeProvider) : IRefreshAuthHandler
{
    public async Task<AuthResult> HandleAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var (user, existing) = await authSessionService.GetActiveRefreshSessionAsync(
            refreshToken,
            cancellationToken);

        var now = timeProvider.GetUtcNow();
        var revokedCount = await refreshTokens.TryRevokeIfActiveAsync(existing.Id, now, cancellationToken);
        if (revokedCount != 1)
            throw new AuthenticationFailedException();

        return await authSessionService.IssueForUserAsync(user, cancellationToken);
    }
}
