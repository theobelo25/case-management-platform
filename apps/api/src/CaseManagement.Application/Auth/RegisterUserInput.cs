namespace CaseManagement.Application.Auth;
public sealed record RegisterUserInput(
    string Email,
    string Password,
    string FirstName,
    string LastName);