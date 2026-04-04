namespace CaseManagement.Application.Auth.Ports;

public interface IRefreshTokenValidator
{
    Task<RefreshTokenSession?> ValidateAsync(
        string rawToken,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
