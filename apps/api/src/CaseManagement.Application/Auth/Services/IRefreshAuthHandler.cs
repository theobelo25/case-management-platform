namespace CaseManagement.Application.Auth;

internal interface IRefreshAuthHandler
{
    Task<AuthResult> HandleAsync(string refreshToken, CancellationToken cancellationToken = default);
}
