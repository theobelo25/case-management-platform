using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Auth;

internal interface IAuthSessionService
{
    Task<AuthResult> IssueForUserAsync(User user, CancellationToken cancellationToken = default);

    Task<(User User, RefreshToken Token)> GetActiveRefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
