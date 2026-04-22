using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class InvalidPasswordExceptionMapper(ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(InvalidPasswordException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var badPassword = (InvalidPasswordException)exception;
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status400BadRequest,
            "Bad Request",
            badPassword.Code,
            badPassword.Message,
            cancellationToken);
    }
}
