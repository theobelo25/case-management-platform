namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class ArgumentExceptionMapper(
    ILogger<ArgumentExceptionMapper> logger,
    ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(ArgumentException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var argEx = (ArgumentException)exception;
        logger.LogWarning(argEx, "Bad request (argument)");
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status400BadRequest,
            "Bad Request",
            code: null,
            detail: argEx.Message,
            cancellationToken);
    }
}
