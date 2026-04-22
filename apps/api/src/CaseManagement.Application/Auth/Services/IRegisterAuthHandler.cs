namespace CaseManagement.Application.Auth;

internal interface IRegisterAuthHandler
{
    Task<AuthResult> HandleAsync(RegisterUserInput input, CancellationToken cancellationToken = default);
}
