using CaseManagement.Api.Common.Contracts;
using FluentValidation;

namespace CaseManagement.Api.Validators;

public sealed class PagingQueryValidator : AbstractValidator<PagingQuery>
{
    public PagingQueryValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than 0");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");
    }
}