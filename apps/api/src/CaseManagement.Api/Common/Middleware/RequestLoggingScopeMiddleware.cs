using CaseManagement.Api.Http;
using Microsoft.Extensions.Logging;

namespace CaseManagement.Api.Middleware;

/// <summary>
/// Adds logging scope state so structured logs include the same <c>TraceId</c> as RFC 7807
/// <c>ProblemDetails</c> responses.
/// </summary>
internal sealed class RequestLoggingScopeMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("CaseManagement.Request");

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = RequestTrace.GetTraceId(context);
        using (_logger.BeginScope(new Dictionary<string, object>(2)
               {
                   ["TraceId"] = traceId
               }))
        {
            await next(context);
        }
    }
}
