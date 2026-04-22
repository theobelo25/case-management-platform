using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Exceptions;

namespace CaseManagement.Application.Auth;

internal sealed class LoginAuthHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAuthSessionService authSessionService) : ILoginAuthHandler
{
    public async Task<AuthResult> HandleAsync(LoginUserInput input, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailNormalizedAsync(normalizedEmail, cancellationToken)
            ?? throw new AuthenticationFailedException();

        if (!passwordHasher.Verify(input.Password, user.PasswordHash))
            throw new AuthenticationFailedException();

        return await authSessionService.IssueForUserAsync(user, cancellationToken);
    }
}
