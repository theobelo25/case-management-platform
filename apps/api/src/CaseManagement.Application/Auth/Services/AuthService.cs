using CaseManagement.Application.Auth.Ports;

namespace CaseManagement.Application.Auth;

internal sealed class AuthService : IAuthService
{
    private readonly IRegisterAuthHandler _registerHandler;
    private readonly ILoginAuthHandler _loginHandler;
    private readonly IRefreshAuthHandler _refreshHandler;
    private readonly ILogoutAuthHandler _logoutHandler;

    public AuthService(
        IRegisterAuthHandler registerHandler,
        ILoginAuthHandler loginHandler,
        IRefreshAuthHandler refreshHandler,
        ILogoutAuthHandler logoutHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _refreshHandler = refreshHandler;
        _logoutHandler = logoutHandler;
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterUserInput input,
        CancellationToken cancellationToken = default)
    {
        return await _registerHandler.HandleAsync(input, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(
        LoginUserInput input,
        CancellationToken cancellationToken = default)
    {
        return await _loginHandler.HandleAsync(input, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return await _refreshHandler.HandleAsync(refreshToken, cancellationToken);
    }

    public async Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _logoutHandler.HandleAsync(refreshToken, cancellationToken);
    }
}