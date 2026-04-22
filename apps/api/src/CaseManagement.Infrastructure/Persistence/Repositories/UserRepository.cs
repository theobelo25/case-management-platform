using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CaseManagementDbContext db) : IUserRepository, IUserDisplayNameLookup
{
    public Task<User?> GetByEmailNormalizedAsync(
        string emailNormalized,
        CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.EmailNormalized == emailNormalized, cancellationToken);

    public Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    
    public void Add(User user) => db.Users.Add(user);

    public async Task<string> GetName(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return user.FullName;
            
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, string>();

        var distinct = userIds.Distinct().ToList();
        var rows = await db.Users
            .AsNoTracking()
            .Where(u => distinct.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.Id, r => r.FullName);
    }
}