using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(
    CaseManagementDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenPrefixAsync(
        string tokenPrefix,
        CancellationToken cancellationToken = default) =>
        db.RefreshTokens.SingleOrDefaultAsync(
            t => t.TokenPrefix == tokenPrefix,
            cancellationToken);

    public void Add(RefreshToken token) => 
        db.RefreshTokens.Add(token);

    public void Update(RefreshToken token) => 
        db.RefreshTokens.Update(token);
    
    public void Remove(RefreshToken token) =>
        db.RefreshTokens.Remove(token);

    public async Task RevokeAllForUserAsync(
        Guid userId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.RevokedAtUtc, revokedAtUtc),
                cancellationToken);
    }

    public Task<int> TryRevokeIfActiveAsync(
        Guid refreshTokenId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default) =>
        db.RefreshTokens
            .Where(t => t.Id == refreshTokenId && t.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.RevokedAtUtc, revokedAtUtc),
                cancellationToken);
}