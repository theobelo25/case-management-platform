namespace CaseManagement.Api.Common.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(string message) : base(message, StatusCodes.Status400BadRequest)
    {}
}