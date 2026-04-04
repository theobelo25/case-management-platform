namespace CaseManagement.Application.Auth;

public sealed record SignUpRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);
