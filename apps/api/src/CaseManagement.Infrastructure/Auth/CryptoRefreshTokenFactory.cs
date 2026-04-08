using System.Security.Cryptography;
using CaseManagement.Application.Ports;

namespace CaseManagement.Infrastructure.Auth;

public sealed class CryptoRefreshTokenFactory(
    IRefreshTokenHasher hasher) : IRefreshTokenFactory
{
    private const char Separator = '.';

    public RawRefreshToken Create()
    {
        Span<byte> prefixBytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(prefixBytes);
        var tokenPrefix = Convert.ToHexString(prefixBytes);

        Span<byte> secretBytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(secretBytes);
        var secret = Convert.ToBase64String(secretBytes);
        
        var token = $"{tokenPrefix}.{secret}";

        var tokenHash = hasher.Hash(token);
        
        return new RawRefreshToken(token, tokenPrefix, tokenHash);
    }
}