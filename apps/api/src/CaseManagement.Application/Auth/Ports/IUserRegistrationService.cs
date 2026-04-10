using CaseManagement.Application.Auth;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface IUserRegistrationService
{
    Task<User> Register(
        RegisterUserInput input,
        CancellationToken cancellationToken = default);
}