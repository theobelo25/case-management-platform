namespace CaseManagement.Application.Auth;

internal interface ILoginAuthHandler
{
    Task<AuthResult> HandleAsync(LoginUserInput input, CancellationToken cancellationToken = default);
}
