using CaseManagement.Api.Demo;
using CaseManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Api.Extensions;

internal static class WebApplicationDatabaseExtensions
{
    internal static async Task ApplyDatabaseMigrationAndDemoSeedAsync(this WebApplication app)
    {
        var configuration = app.Configuration;

        if (configuration.GetValue("Database:ApplyMigrationsOnStart", false))
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CaseManagementDbContext>();
            await db.Database.MigrateAsync();
        }

        if (!configuration.GetValue("Demo:Seed", false) || !app.Environment.IsDevelopment())
            return;

        await using var seedScope = app.Services.CreateAsyncScope();
        await DemoDataSeeder.TrySeedAsync(seedScope.ServiceProvider, configuration);
    }
}
