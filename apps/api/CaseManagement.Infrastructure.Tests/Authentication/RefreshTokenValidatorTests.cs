using System.Security.Cryptography;
using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Infrastructure.Authentication;
using Moq;

namespace CaseManagement.Infrastructure.Tests.Authentication;

/// <summary>
/// Tests for refresh token parsing and hash verification (lookup id + secret, compared to stored SHA-256).
/// </summary>
public sealed class RefreshTokenValidatorTests
{
    private static string Base64UrlEncode(ReadOnlySpan<byte> data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    [Fact]
    public async Task ValidateAsync_valid_token_returns_session()
    {
        var lookupId = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        Span<byte> secret = stackalloc byte[32];
        RandomNumberGenerator.Fill(secret);
        var hash = SHA256.HashData(secret);
        var raw = $"{lookupId}.{Base64UrlEncode(secret)}";

        var session = new RefreshTokenSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            hash,
            DateTime.UtcNow.AddDays(1),
            null,
            null);

        var store = new Mock<IRefreshTokenStore>(MockBehavior.Strict);
        store
            .Setup(s => s.GetActiveByLookupIdAsync(lookupId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sut = new RefreshTokenValidator(store.Object);

        var result = await sut.ValidateAsync(raw, DateTime.UtcNow, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(session.Id, result.Id);
    }

    [Fact]
    public async Task ValidateAsync_wrong_secret_returns_null()
    {
        var lookupId = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        Span<byte> secret = stackalloc byte[32];
        RandomNumberGenerator.Fill(secret);
        var wrongSecret = new byte[32];
        Array.Fill(wrongSecret, (byte)1);
        var hash = SHA256.HashData(secret);
        var raw = $"{lookupId}.{Base64UrlEncode(wrongSecret)}";

        var session = new RefreshTokenSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            hash,
            DateTime.UtcNow.AddDays(1),
            null,
            null);

        var store = new Mock<IRefreshTokenStore>();
        store
            .Setup(s => s.GetActiveByLookupIdAsync(lookupId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sut = new RefreshTokenValidator(store.Object);

        var result = await sut.ValidateAsync(raw, DateTime.UtcNow, CancellationToken.None);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("noseparator")]
    [InlineData(".onlysecret")]
    public async Task ValidateAsync_malformed_raw_returns_null(string raw)
    {
        var store = new Mock<IRefreshTokenStore>(MockBehavior.Strict);
        var sut = new RefreshTokenValidator(store.Object);

        var result = await sut.ValidateAsync(raw, DateTime.UtcNow, CancellationToken.None);

        Assert.Null(result);
    }
}
