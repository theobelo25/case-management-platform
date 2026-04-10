using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CaseManagementDbContext db) : IUserRepository
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
}