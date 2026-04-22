using FluentValidation;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class ValidationExceptionMapper(
    ILogger<ValidationExceptionMapper> logger,
    ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(ValidationException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var ve = (ValidationException)exception;
        logger.LogWarning(ve, "Validation failed");
        return writer.WriteValidationProblemAsync(httpContext, ve, cancellationToken);
    }
}
