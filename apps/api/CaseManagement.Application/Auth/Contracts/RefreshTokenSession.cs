namespace CaseManagement.Application.Auth;

public sealed record RefreshTokenSession(
    Guid Id,
    Guid UserId,
    Guid FamilyId,
    byte[] TokenHash,
    DateTime ExpiresAtUtc,
    string? ClientUserAgent,
    string? ClientIpAddress);
