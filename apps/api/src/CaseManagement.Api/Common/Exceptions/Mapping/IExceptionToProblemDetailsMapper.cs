namespace CaseManagement.Api.Exceptions.Mapping;

/// <summary>
/// Maps a concrete exception type to an HTTP problem response. Register one implementation per exception type;
/// the registry resolves the most specific registered type by walking the exception's inheritance chain.
/// </summary>
public interface IExceptionToProblemDetailsMapper
{
    Type ExceptionType { get; }

    Task HandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken);
}
