using CaseManagement.Application.Auth;
using CaseManagement.Domain.Users;
using CaseManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PasswordHash = user.PasswordHash,
            CreatedAtUtc = user.CreatedAtUtc
        };
        _db.Users.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> AnyUsersAsync(CancellationToken cancellationToken) =>
        _db.Users.AnyAsync(cancellationToken);

    private static User ToDomain(UserEntity entity) =>
        new(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.PasswordHash,
            entity.CreatedAtUtc);
}