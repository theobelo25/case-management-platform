namespace CaseManagement.Application.Auth;

public interface IAuthService
{
    public Task<AuthResult> RegisterAsync(
        RegisterUserInput input,
        CancellationToken cancellationToken = default);

    public Task<AuthResult> LoginAsync(
        LoginUserInput input,
        CancellationToken cancellationToken = default);

    public Task<AuthResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    public Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default);
}
