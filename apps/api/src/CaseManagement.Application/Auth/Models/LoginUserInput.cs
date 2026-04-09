namespace CaseManagement.Application.Auth;
public sealed record LoginUserInput(
    string Email,
    string Password);