using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common.Exceptions;
using CaseManagement.Domain.Users;
using Moq;

namespace CaseManagement.Application.Tests.Auth;

/// <summary>
/// Unit tests for the refresh-token slice of <see cref="AuthService"/>:
/// invalid tokens are rejected uniformly, and a valid session is rotated with a new access token.
/// </summary>
public sealed class AuthServiceRefreshTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IRefreshTokenIssuer> _issuer = new();
    private readonly Mock<IRefreshTokenValidator> _validator = new();
    private readonly Mock<IRefreshTokenStore> _store = new();

    private AuthService CreateSut() => new(
        _users.Object,
        _passwordHasher.Object,
        _jwt.Object,
        _issuer.Object,
        _validator.Object,
        _store.Object);

    [Fact]
    public async Task RefreshAsync_when_session_invalid_throws_UnauthorizedException()
    {
        _validator
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenSession?)null);

        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            sut.RefreshAsync("any.raw", null, null, CancellationToken.None));

        Assert.Equal("Invalid or expired refresh token.", ex.Message);
        _issuer.Verify(
            i => i.RotateAsync(
                It.IsAny<RefreshTokenSession>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshAsync_when_token_empty_still_asks_validator(string? raw)
    {
        _validator
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenSession?)null);

        var sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            sut.RefreshAsync(raw, null, null, CancellationToken.None));

        _validator.Verify(
            v => v.ValidateAsync(
                raw ?? string.Empty,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_when_user_no_longer_exists_throws_UnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var session = new RefreshTokenSession(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            new byte[32],
            DateTime.UtcNow.AddDays(1),
            null,
            null);

        _validator
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _users
            .Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            sut.RefreshAsync("lookup.secret", null, null, CancellationToken.None));

        _issuer.Verify(
            i => i.RotateAsync(
                It.IsAny<RefreshTokenSession>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_success_issues_access_token_and_rotates_refresh_session()
    {
        var userId = Guid.NewGuid();
        var session = new RefreshTokenSession(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            new byte[32],
            DateTime.UtcNow.AddDays(1),
            null,
            null);
        var user = new User(
            userId,
            "dev@example.com",
            "Dev",
            "User",
            "hash",
            DateTime.UtcNow);

        _validator
            .Setup(v => v.ValidateAsync(
                "present",
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _users
            .Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var access = new JwtTokenResult
        {
            AccessToken = "jwt-access",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
        };
        _jwt
            .Setup(j => j.CreateAccessToken(userId, user.Email, user.FullName))
            .Returns(access);

        var refreshExpires = DateTime.UtcNow.AddDays(14);
        _issuer
            .Setup(i => i.RotateAsync(session, "MyAgent", "203.0.113.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("next-refresh-raw", refreshExpires));

        var sut = CreateSut();

        var result = await sut.RefreshAsync("present", "MyAgent", "203.0.113.1", CancellationToken.None);

        Assert.Equal("jwt-access", result.Auth.AccessToken);
        Assert.Equal(access.ExpiresAtUtc, result.Auth.ExpiresAtUtc);
        Assert.Equal(userId, result.Auth.UserId);
        Assert.Equal("next-refresh-raw", result.RefreshToken);
        Assert.Equal(refreshExpires, result.RefreshTokenExpiresAtUtc);

        _issuer.Verify(
            i => i.RotateAsync(session, "MyAgent", "203.0.113.1", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
