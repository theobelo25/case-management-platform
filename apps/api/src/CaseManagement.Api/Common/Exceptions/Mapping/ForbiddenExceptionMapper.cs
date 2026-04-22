using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class ForbiddenExceptionMapper(
    ILogger<ForbiddenExceptionMapper> logger,
    ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(ForbiddenException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var forbidden = (ForbiddenException)exception;
        logger.LogWarning(forbidden, "Forbidden");
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status403Forbidden,
            "Forbidden",
            forbidden.Code,
            forbidden.Message,
            cancellationToken);
    }
}
