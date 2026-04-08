namespace CaseManagement.Application.Exceptions;

public sealed class BadRequestArgumentException : ArgumentException
{
    public string? Code { get; }
    public BadRequestArgumentException(string message, string? code = null)
        : base(message)
    {
        Code = code;
    }
    public BadRequestArgumentException(string message, string paramName, string? code = null)
        : base(message, paramName)
    {
        Code = code;
    }
    public BadRequestArgumentException(string message, Exception innerException, string? code = null)
        : base(message, innerException)
    {
        Code = code;
    }
    public BadRequestArgumentException(
        string message,
        string paramName,
        Exception innerException,
        string? code = null)
        : base(message, paramName, innerException)
    {
        Code = code;
    }
}