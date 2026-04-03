namespace CaseManagement.Application.Auth.Ports;

public interface IRefreshTokenStore
{
    Task<Guid> CreateAsync(
        Guid userId,
        byte[] tokenHash,
        string lookupId,
        DateTime expiresAtUtc,
        Guid? familyId,
        Guid? previousSessionId,
        string? clientUserAgent = null,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default);
    Task<RefreshTokenSession?> GetActiveByLookupIdAsync(
        string lookupId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
    Task RevokeAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}