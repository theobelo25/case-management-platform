using FluentValidation;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetails)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException ve:
                logger.LogWarning(ve, "Validation failed");
                await WriteValidationProblemAsync(httpContext, ve, cancellationToken);
                return true;
            case ConflictException conflict:
                await WriteAppProblemAsync(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    conflict.Code,
                    conflict.Message,
                    cancellationToken);
                return true;
            case AuthenticationFailedException auth:
                await WriteAppProblemAsync(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    auth.Code,
                    auth.Message,
                    cancellationToken);
                return true;
            case NotFoundException notFound:
                await WriteAppProblemAsync(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    notFound.Code,
                    notFound.Message,
                    cancellationToken);
                return true;
            case InvalidPasswordException badPassword:
                await WriteAppProblemAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    badPassword.Code,
                    badPassword.Message,
                    cancellationToken);
                return true;
            case BadRequestArgumentException badRequest:
                logger.LogWarning(badRequest, "Bad request");
                await WriteAppProblemAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    badRequest.Code,
                    badRequest.Message,
                    cancellationToken);
                return true;
            default:
                logger.LogError(exception, "Unhandled exception");
                var fallback = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Detail = "A server error occurred. Please try again later.",
                    Instance = httpContext.Request.Path
                };
                await WriteProblemAsync(httpContext, fallback, cancellationToken);
                return true;
        }
    }

    private async Task WriteValidationProblemAsync(
        HttpContext httpContext,
        ValidationException ve,
        CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["errors"] = ve.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        await WriteProblemAsync(httpContext, problem, cancellationToken);
    }

    private async Task WriteAppProblemAsync(
        HttpContext httpContext,
        int status,
        string title,
        string? code,
        string detail,
        CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        if (!string.IsNullOrEmpty(code))
            problem.Extensions["code"] = code;
        await WriteProblemAsync(httpContext, problem, cancellationToken);
    }

    private async Task WriteProblemAsync(
        HttpContext httpContext,
        ProblemDetails problem,
        CancellationToken cancellationToken)
    {
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var status = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.StatusCode = status;
        await problemDetails.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });
    }
}