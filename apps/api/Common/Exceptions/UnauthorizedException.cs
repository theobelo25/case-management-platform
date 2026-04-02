namespace CaseManagement.Api.Common.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException() : base("Unauthorized.", StatusCodes.Status401Unauthorized)
    {}

    public UnauthorizedException(string message) : base(message, StatusCodes.Status401Unauthorized)
    {}
}