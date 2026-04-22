namespace CaseManagement.Application.Auth.Ports;

public sealed record PersistedRefreshToken(string Token, DateTimeOffset ExpiresAtUtc);

public interface IRefreshTokenPersistence
{
    Task<PersistedRefreshToken> AddForUserAsync(
        Guid userId,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default);
}