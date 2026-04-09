using CaseManagement.Application.Auth;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface IUserRegistration
{
    Task<User> Register(
        RegisterUserInput input,
        CancellationToken cancellationToken = default);
}