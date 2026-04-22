using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class AuthenticationFailedExceptionMapper(ExceptionProblemDetailsWriter writer)
    : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(AuthenticationFailedException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var auth = (AuthenticationFailedException)exception;
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status401Unauthorized,
            "Unauthorized",
            auth.Code,
            auth.Message,
            cancellationToken);
    }
}
