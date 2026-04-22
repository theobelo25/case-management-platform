using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence;

public sealed class EfRefreshTokenPersistence(
    IRefreshTokenFactory refreshTokenFactory,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork) : IRefreshTokenPersistence
{
    private const int MaxAttempts = 3;

    public async Task<PersistedRefreshToken> AddForUserAsync(
        Guid userId,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var raw = refreshTokenFactory.Create();

            var entity = RefreshToken.Issue(
                Guid.NewGuid(),
                userId,
                raw.TokenPrefix,
                raw.TokenHash,
                expiresAtUtc,
                createdAtUtc);

            refreshTokens.Add(entity);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return new PersistedRefreshToken(raw.Token, expiresAtUtc);
            }
            catch (DbUpdateException) when (attempt < MaxAttempts - 1)
            {
                refreshTokens.Remove(entity);
            }
        }

        throw new InvalidOperationException(
            "Failed to persist refresh token after multiple attempts.");
    }
}