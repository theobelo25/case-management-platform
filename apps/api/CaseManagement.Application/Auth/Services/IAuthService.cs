namespace CaseManagement.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken = default);

    Task<MeResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
