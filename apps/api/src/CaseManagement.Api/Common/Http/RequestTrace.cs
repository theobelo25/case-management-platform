using System.Diagnostics;
using CaseManagement.Api.Exceptions.Mapping;

namespace CaseManagement.Api.Http;

/// <summary>
/// Correlates logs and <c>ProblemDetails</c> responses using the same identifier as
/// <see cref="ExceptionProblemDetailsWriter"/>.
/// </summary>
public static class RequestTrace
{
    public static string GetTraceId(HttpContext httpContext) =>
        Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
