namespace CaseManagement.Application.Auth.Ports;

public interface IRefreshTokenIssuer
{
    Task<(string RawToken, DateTime ExpiresAtUtc)> IssueAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<(string RawToken, DateTime ExpiresAtUtc)> RotateAsync(
        RefreshTokenSession previousSession,
        string? clientUserAgent,
        string? clientIpAddress,
        CancellationToken cancellationToken = default);
}