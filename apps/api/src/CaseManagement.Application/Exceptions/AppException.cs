namespace CaseManagement.Application.Exceptions;

public static class AppErrorCodes
{
    public const string DuplicateEmail = "duplicate_email";
    public const string AuthFailed = "auth_failed";
    public const string UserNotFound = "user_not_found";
    public const string PasswordPolicy = "password_policy";
    public const string MembershipNotFound = "membership_not_found";
    public const string NoActiveOrganization = "no_active_organization";
    public const string OrganizationNotFound = "organization_not_found";
}

public abstract class AppException : Exception
{
    public string? Code { get; }
    protected AppException(
        string message, 
        string? code = null, 
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }
}

public sealed class ConflictException : AppException
{
    public ConflictException(
        string message, 
        string? code = null, 
        Exception? innerException = null)
        : base(message, code, innerException) {}
}

public sealed class AuthenticationFailedException : AppException
{
    public AuthenticationFailedException(
        string message = "Authentication failed. Please check credentials.",  
        Exception? innerException = null)
        : base(message, AppErrorCodes.AuthFailed, innerException) { }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(
        string message, 
        string? code = null, 
        Exception? innerException = null)
        : base(message, code, innerException) { }
}

public sealed class InvalidPasswordException : AppException
{
    public InvalidPasswordException(
        string message,
        string? code = null,
        Exception? innerException = null)
        : base(message, code ?? AppErrorCodes.PasswordPolicy, innerException) { }
}