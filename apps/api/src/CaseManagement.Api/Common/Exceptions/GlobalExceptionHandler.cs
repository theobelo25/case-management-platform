using CaseManagement.Api.Exceptions.Mapping;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IExceptionToProblemDetailsMapperRegistry mapperRegistry,
    DevelopmentDatabaseExceptionResponder developmentDatabaseResponder,
    ExceptionProblemDetailsWriter problemWriter)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var mapper = mapperRegistry.GetMapperFor(exception);
        if (mapper is not null)
        {
            await mapper.HandleAsync(httpContext, exception, cancellationToken);
            return true;
        }

        if (await developmentDatabaseResponder.TryWriteAsync(httpContext, exception, cancellationToken))
            return true;

        logger.LogError(exception, "Unhandled exception");
        var fallback = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Detail = "A server error occurred. Please try again later.",
            Instance = httpContext.Request.Path
        };
        await problemWriter.WriteProblemAsync(httpContext, fallback, cancellationToken);
        return true;
    }
}
