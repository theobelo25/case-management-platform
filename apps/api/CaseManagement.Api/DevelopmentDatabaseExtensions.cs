using CaseManagement.Application.Database;

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
