using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class BadRequestArgumentExceptionMapper(
    ILogger<BadRequestArgumentExceptionMapper> logger,
    ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(BadRequestArgumentException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var badRequest = (BadRequestArgumentException)exception;
        logger.LogWarning(badRequest, "Bad request");
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status400BadRequest,
            "Bad Request",
            badRequest.Code,
            badRequest.Message,
            cancellationToken);
    }
}
