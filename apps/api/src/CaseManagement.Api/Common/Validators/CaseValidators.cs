using CaseManagement.Api.Cases.Contracts;
using FluentValidation;

namespace CaseManagement.Api.Validators;

public sealed class CreateCaseRequestValidator : AbstractValidator<CreateCaseRequest>
{
    public CreateCaseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.InitialMessage)
            .NotEmpty().WithMessage("Initial message is required.")
            .MaximumLength(10000).WithMessage("Initial message must be at most 10000 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be LOW, MEDIUM, or HIGH.");
    }
}

public sealed class AddCaseCommentRequestValidator : AbstractValidator<AddCaseCommentRequest>
{
    public AddCaseCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required.")
            .MaximumLength(10000).WithMessage("Comment must be at most 10000 characters.");
    }
}
