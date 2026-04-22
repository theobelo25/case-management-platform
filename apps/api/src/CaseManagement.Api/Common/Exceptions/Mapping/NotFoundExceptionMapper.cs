using CaseManagement.Application.Exceptions;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class NotFoundExceptionMapper(ExceptionProblemDetailsWriter writer) : IExceptionToProblemDetailsMapper
{
    public Type ExceptionType => typeof(NotFoundException);

    public Task HandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var notFound = (NotFoundException)exception;
        return writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status404NotFound,
            "Not Found",
            notFound.Code,
            notFound.Message,
            cancellationToken);
    }
}
