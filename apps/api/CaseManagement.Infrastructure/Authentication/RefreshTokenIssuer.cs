using System.Security.Cryptography;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Options;
using CaseManagement.Application.Auth.Ports;
using Microsoft.Extensions.Options;

namespace CaseManagement.Infrastructure.Authentication;

public sealed class RefreshTokenIssuer : IRefreshTokenIssuer
{
    private readonly IRefreshTokenStore _store;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenIssuer(
        IRefreshTokenStore store,
        IOptions<RefreshTokenOptions> options)
    {
        _store = store;
        _options = options.Value;
    }

    public async Task<(string RawToken, DateTime ExpiresAtUtc)> IssueAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var expiresAtUtc = DateTime.UtcNow.AddDays(_options.ExpiryDays);

        Span<byte> secret = stackalloc byte[32];
        RandomNumberGenerator.Fill(secret);

        Span<byte> lookupRaw = stackalloc byte[16];
        RandomNumberGenerator.Fill(lookupRaw);
        var lookupId = Convert.ToHexString(lookupRaw);

        var tokenHash = SHA256.HashData(secret);
        var secretEncoded = Base64UrlEncode(secret);

        await _store.CreateAsync(
            userId,
            tokenHash,
            lookupId,
            expiresAtUtc,
            familyId: null,
            previousSessionId: null,
            clientUserAgent: null,
            clientIpAddress: null,
            cancellationToken);

        var rawToken = $"{lookupId}.{secretEncoded}";
        return (rawToken, expiresAtUtc);
    }

    public async Task<(string RawToken, DateTime ExpiresAtUtc)> RotateAsync(
        RefreshTokenSession previousSession,
        string? clientUserAgent,
        string? clientIpAddress,
        CancellationToken cancellationToken = default)
    {
        var expiresAtUtc = DateTime.UtcNow.AddDays(_options.ExpiryDays);
        Span<byte> secret = stackalloc byte[32];
        RandomNumberGenerator.Fill(secret);
        Span<byte> lookupRaw = stackalloc byte[16];
        RandomNumberGenerator.Fill(lookupRaw);
        var lookupId = Convert.ToHexString(lookupRaw);
        var tokenHash = SHA256.HashData(secret);
        var secretEncoded = Base64UrlEncode(secret);
        await _store.CreateAsync(
            previousSession.UserId,
            tokenHash,
            lookupId,
            expiresAtUtc,
            previousSession.FamilyId,
            previousSession.Id,
            clientUserAgent,
            clientIpAddress,
            cancellationToken);
        var rawToken = $"{lookupId}.{secretEncoded}";
        return (rawToken, expiresAtUtc);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> data)
    {
        // Standard base64url, no padding
        var s = Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return s;
    }
}