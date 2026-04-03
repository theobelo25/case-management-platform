namespace CaseManagement.Infrastructure.Persistence.Entities;

public sealed class RefreshSessionEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }
    public string LookupId { get; set; } = string.Empty;
    public byte[] TokenHash { get; set; } = [];
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? ClientUserAgent { get; set; }
    public string? ClientIpAddress { get; set; }
    public Guid? ReplacedBySessionId { get; set; }

    public UserEntity? User { get; set; }
    public RefreshSessionEntity? ReplacedBySession { get; set; }
}