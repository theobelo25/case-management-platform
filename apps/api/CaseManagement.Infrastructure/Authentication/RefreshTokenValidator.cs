using System.Security.Cryptography;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;

namespace CaseManagement.Infrastructure.Authentication;

public sealed class RefreshTokenValidator : IRefreshTokenValidator
{
    private readonly IRefreshTokenStore _store;

    public RefreshTokenValidator(IRefreshTokenStore store) => _store = store;

    public async Task<RefreshTokenSession?> ValidateAsync(
        string rawToken,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return null;
        
        var dot = rawToken.IndexOf(".");
        if (dot <= 0 || dot == rawToken.Length - 1)
            return null;

        var lookupId = rawToken[..dot];
        byte[] secret;
        try
        {
            secret = Base64UrlDecode(rawToken[(dot + 1)..]);
        }
        catch
        {
            return null;
        }

        if (secret.Length != 32)
            return null;
        
        var session = await _store.GetActiveByLookupIdAsync(lookupId, utcNow, cancellationToken);
        if (session is null)
            return null;

        var hash = SHA256.HashData(secret);
        if (!CryptographicOperations.FixedTimeEquals(hash, session.TokenHash))
            return null;

        return session;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}