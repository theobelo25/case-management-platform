using System.Data.Common;
using Microsoft.Extensions.Hosting;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class DevelopmentDatabaseExceptionResponder(
    IHostEnvironment environment,
    ILogger<DevelopmentDatabaseExceptionResponder> logger,
    ExceptionProblemDetailsWriter writer)
{
    public async Task<bool> TryWriteAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return false;

        if (FindDbException(exception) is not { } dbEx)
            return false;

        logger.LogError(exception, "Database error (development detail exposed)");
        await writer.WriteAppProblemAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Database error",
            code: null,
            detail: dbEx.Message,
            cancellationToken);
        return true;
    }

    private static DbException? FindDbException(Exception exception)
    {
        for (Exception? e = exception; e is not null; e = e.InnerException)
        {
            if (e is DbException dbe)
                return dbe;
        }

        return null;
    }
}
