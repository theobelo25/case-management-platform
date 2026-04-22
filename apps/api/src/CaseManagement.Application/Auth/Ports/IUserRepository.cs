using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Auth.Ports;

public interface IUserRepository
{
    Task<User?> GetByEmailNormalizedAsync(string emailNormalized, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(User user);
}