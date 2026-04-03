namespace CaseManagement.Application.Auth;

public sealed record SignInRequest(
    string Email,
    string Password
);
