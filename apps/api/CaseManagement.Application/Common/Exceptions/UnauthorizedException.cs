namespace CaseManagement.Application.Common.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized.") 
        : base(message, AppHttpStatus.Unauthorized) {}
}