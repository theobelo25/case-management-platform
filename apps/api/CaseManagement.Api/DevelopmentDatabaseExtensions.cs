using CaseManagement.Application.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CaseManagement.Api;

public static class DevelopmentDatabaseExtensions
{
    public static async Task InitializeDatabaseInDevelopmentAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        if (!app.Environment.IsDevelopment())
            return;

        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDevelopmentDatabaseInitializer>();
        await initializer.InitializeAsync(cancellationToken);
    }
}
