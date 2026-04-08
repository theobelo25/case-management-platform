namespace CaseManagement.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    public string TokenPrefix { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    private RefreshToken() {}

    public static RefreshToken Issue(
        Guid id,
        Guid userId,
        string tokenPrefix,
        string tokenHash,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new RefreshToken
        {
            Id = id,
            UserId = userId,
            TokenPrefix = tokenPrefix,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = createdAtUtc,
            RevokedAtUtc = null
        };
    }

    public bool IsActive(DateTimeOffset utcNow) =>
        RevokedAtUtc is null && ExpiresAtUtc > utcNow;

    public void Revoke(DateTimeOffset revokedAtUtc) =>
        RevokedAtUtc = revokedAtUtc;
}