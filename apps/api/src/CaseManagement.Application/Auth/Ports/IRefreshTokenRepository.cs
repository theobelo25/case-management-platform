using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Auth.Ports;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenPrefixAsync(
        string tokenPrefix, 
        CancellationToken cancellationToken = default);
    void Add(RefreshToken token);
    void Update(RefreshToken token);
    void Remove (RefreshToken token);
    Task RevokeAllForUserAsync(
        Guid userId, 
        DateTimeOffset revokedAtUtc, 
        CancellationToken cancellationToken = default);
    Task<int> TryRevokeIfActiveAsync(
        Guid refreshTokenId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default);
}