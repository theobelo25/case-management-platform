using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class ConflictExceptionMapper(ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(ConflictException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var conflict = (ConflictException)exception;
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status409Conflict,
            "Conflict",
            conflict.Code,
            conflict.Message,
            cancellationToken);
    }
}
