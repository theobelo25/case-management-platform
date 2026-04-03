using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Database;

namespace CaseManagement.Infrastructure.Persistence;

internal sealed class DevelopmentDatabaseInitializer : IDevelopmentDatabaseInitializer
{
    private readonly AppDbContext _db;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public DevelopmentDatabaseInitializer(
        AppDbContext db,
        IUserRepository users,
        IPasswordHasher hasher)
    {
        _db = db;
        _users = users;
        _hasher = hasher;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        DbInitializer.SeedAsync(_db, _users, _hasher, cancellationToken);
}
