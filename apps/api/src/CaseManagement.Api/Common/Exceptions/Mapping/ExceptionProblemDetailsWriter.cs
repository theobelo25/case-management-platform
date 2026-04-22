using CaseManagement.Api.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class ExceptionProblemDetailsWriter(IProblemDetailsService problemDetails)
{
    public async Task WriteAppProblemAsync(
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

    public async Task WriteValidationProblemAsync(
        HttpContext httpContext,
        FluentValidation.ValidationException ve,
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

    public async Task WriteProblemAsync(
        HttpContext httpContext,
        ProblemDetails problem,
        CancellationToken cancellationToken)
    {
        problem.Extensions["traceId"] = RequestTrace.GetTraceId(httpContext);
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
