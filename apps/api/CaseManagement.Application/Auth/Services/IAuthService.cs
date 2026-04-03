namespace CaseManagement.Application.Auth;

public interface IAuthService
{
    Task<SignInResult> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken = default);

    Task<MeResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SignInResult> RefreshAsync(
        string? refreshTokenRaw,
        string? clientUserAgent,
        string? clientIpAddress,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(
        string? refreshTokenRaw,
        bool revokeAllSessions,
        Guid? authenticatedUserId,
        CancellationToken cancellationToken = default);
}
