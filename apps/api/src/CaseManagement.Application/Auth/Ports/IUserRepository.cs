using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface IUserRepository
{
    Task<User?> GetByEmailNormalizedAsync(string emailNormalized, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(User user);
    Task<string> GetName(
        Guid userId,
        CancellationToken cancellationToken = default);
}