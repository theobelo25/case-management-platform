using CaseManagement.Application.Auth;
using FluentValidation;

namespace CaseManagement.Api.Validation;

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(1024);
    }
}
