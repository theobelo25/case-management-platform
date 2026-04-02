using Microsoft.AspNetCore.Http.HttpResults;

namespace CaseManagement.Api.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {}
}