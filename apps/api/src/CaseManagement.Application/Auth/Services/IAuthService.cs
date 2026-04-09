namespace CaseManagement.Application.Auth;

public interface IAuthService
{
    public Task<AuthResult> RegisterAsync(
        RegisterUserInput input, 
        CancellationToken ct = default);

    public Task<AuthResult> LoginAsync(
        LoginUserInput input, 
        CancellationToken ct = default);

    public Task<AuthResult> RefreshAsync(
        string refreshToken, 
        CancellationToken ct = default);

    public Task LogoutAsync(
        string? refreshToken, 
        CancellationToken ct = default);
}
