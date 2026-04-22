using CaseManagement.Application.Auth.Ports;

namespace CaseManagement.Application.Auth;

internal sealed class LogoutAuthHandler(
    IRefreshTokenRepository refreshTokens,
    IAuthSessionService authSessionService,
    TimeProvider timeProvider) : ILogoutAuthHandler
{
    public async Task HandleAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var (_, existing) = await authSessionService.GetActiveRefreshSessionAsync(
            refreshToken,
            cancellationToken);

        var now = timeProvider.GetUtcNow();
        await refreshTokens.TryRevokeIfActiveAsync(existing.Id, now, cancellationToken);
    }
}
