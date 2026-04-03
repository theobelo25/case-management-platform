namespace CaseManagement.Application.Common.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(string message) 
        : base(message, AppHttpStatus.BadRequest) {}
}