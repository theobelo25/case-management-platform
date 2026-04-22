namespace CaseManagement.Application.Users.Ports;

/// <summary>
/// Read-only resolution of user display names for presentation (timeline, notifications, etc.).
/// Kept separate from <see cref="CaseManagement.Application.Auth.Ports.IUserRepository"/> (aggregate persistence) per ISP.
/// </summary>
public interface IUserDisplayNameLookup
{
    Task<string> GetName(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
}
