namespace CaseManagement.Application.Common.Exceptions;

public sealed class TooManyRequestsException : AppException
{
    public TooManyRequestsException(string message = "Too many requests.")
        : base(message, AppHttpStatus.TooManyRequests) { }
}
