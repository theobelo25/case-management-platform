using CaseManagement.Domain.Users;

namespace CaseManagement.Application.Auth;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> AnyUsersAsync(CancellationToken cancellationToken = default);
}