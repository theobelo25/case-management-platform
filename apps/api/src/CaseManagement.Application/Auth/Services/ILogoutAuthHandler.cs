namespace CaseManagement.Application.Auth;

internal interface ILogoutAuthHandler
{
    Task HandleAsync(string? refreshToken, CancellationToken cancellationToken = default);
}
