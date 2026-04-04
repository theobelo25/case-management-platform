using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _db;

    public RefreshTokenStore(AppDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(
        Guid userId,
        byte[] tokenHash,
        string lookupId,
        DateTime expiresAtUtc,
        Guid? familyId,
        Guid? previousSessionId,
        string? clientUserAgent = null,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;

        if (previousSessionId is null)
        {
            var row = new RefreshSessionEntity
            {
                Id = id,
                UserId = userId,
                FamilyId = Guid.NewGuid(),
                LookupId = lookupId,
                TokenHash = tokenHash,
                ExpiresAtUtc = expiresAtUtc,
                CreatedAtUtc = utcNow,
                ClientUserAgent = clientUserAgent,
                ClientIpAddress = clientIpAddress,
            };

            _db.RefreshSessions.Add(row);
        }
        else
        {
            var previous = await _db.RefreshSessions
                .FirstOrDefaultAsync(x => x.Id == previousSessionId, cancellationToken)
                ?? throw new InvalidOperationException("Previous refresh session not found.");

            if (familyId is not Guid fid || fid != previous.FamilyId)
                throw new InvalidOperationException("FamilyId must match the previous session.");

            previous.RevokedAtUtc = utcNow;
            previous.ReplacedBySessionId = id;

            _db.RefreshSessions.Add(new RefreshSessionEntity
            {
                Id = id,
                UserId = userId,
                FamilyId = fid,
                LookupId = lookupId,
                TokenHash = tokenHash,
                ExpiresAtUtc = expiresAtUtc,
                CreatedAtUtc = utcNow,
                ClientUserAgent = clientUserAgent,
                ClientIpAddress = clientIpAddress,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<RefreshTokenSession?> GetActiveByLookupIdAsync(
        string lookupId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.RefreshSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LookupId == lookupId
                    && x.RevokedAtUtc == null
                    && x.ExpiresAtUtc > utcNow,
                cancellationToken);

        return row is null
            ? null
            : new RefreshTokenSession(
                row.Id,
                row.UserId,
                row.FamilyId,
                row.TokenHash,
                row.ExpiresAtUtc,
                row.ClientUserAgent,
                row.ClientIpAddress);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        await _db.RefreshSessions
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.RevokedAtUtc, utcNow),
                cancellationToken);
    }

    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        await _db.RefreshSessions
            .Where(x => x.Id == sessionId && x.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.RevokedAtUtc, utcNow),
                cancellationToken);
    }
}
