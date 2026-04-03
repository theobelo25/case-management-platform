using CaseManagement.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail, isApp) = Map(exception);

        if (isApp)
            logger.LogWarning(exception, "Application exception: {Message}", detail);
        else
            logger.LogError(exception, "Unhandled exception");

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path.Value
        };

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }

    private static (int Status, string Title, string Detail, bool IsAppException) Map(
        Exception exception)
    {
        if (exception is AppException app)
            return (app.StatusCode, TitleFor(app.StatusCode), app.Message, true);

        return (
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred.",
            false);
    }

    private static string TitleFor(int status) => status switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        429 => "Too Many Requests",
        422 => "Unprocessable Entity",
        _ => "Error"
    };
}
