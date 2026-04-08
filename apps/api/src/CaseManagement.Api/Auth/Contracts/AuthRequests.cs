namespace CaseManagement.Api.Contracts;

public sealed record RegisterRequest(
    string Email, 
    string Password, 
    string ConfirmPassword, 
    string FirstName, 
    string LastName)
{
    public string EmailForValidation => Email.Trim();
    public string FirstNameForValidation => FirstName.Trim();
    public string LastNameForValidation => LastName.Trim();
}

public sealed record LoginRequest(string Email, string Password)
{
    public string EmailForValidation => Email.Trim();
}

