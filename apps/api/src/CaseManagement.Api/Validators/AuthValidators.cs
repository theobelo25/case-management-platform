using System.Text.RegularExpressions;
using CaseManagement.Api.Auth.Contracts;
using FluentValidation;

namespace CaseManagement.Api.Validators;

public sealed class AuthRequestMarker {}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailForValidation)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password is too long.");
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private static readonly Regex NameChars = new(
        @"^[\p{L}\p{M}\p{Zs}'\-]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
    public RegisterRequestValidator()
    {
        RuleFor(x => x.EmailForValidation)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password is too long.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
            
        RuleFor(x => x.FirstNameForValidation)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100)
            .Matches(NameChars).WithMessage("First name contains invalid characters.");

        RuleFor(x => x.LastNameForValidation)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100)
            .Matches(NameChars).WithMessage("Last name contains invalid characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your password.")
            .Equal(x => x.Password).WithMessage("Passwords must match.");
    }
}