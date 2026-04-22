using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Exceptions;

namespace CaseManagement.Application.Auth;

internal sealed class RegisterAuthHandler(
    IUserRepository users,
    IUserRegistrationService userRegistration,
    IAuthSessionService authSessionService) : IRegisterAuthHandler
{
    public async Task<AuthResult> HandleAsync(RegisterUserInput input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Password))
            throw new BadRequestArgumentException("Password is required.");

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailNormalizedAsync(normalizedEmail, cancellationToken) is not null)
            throw new ConflictException("Email already registered.", code: AppErrorCodes.DuplicateEmail);

        var user = await userRegistration.Register(input, cancellationToken);
        return await authSessionService.IssueForUserAsync(user, cancellationToken);
    }
}
