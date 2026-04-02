using CaseManagement.Api.Features.Auth.Contracts;

namespace CaseManagement.Api.Features.Auth.Services;

public interface IAuthService
{
    Task<AuthResponse> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken = default);
    
    Task<MeResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}