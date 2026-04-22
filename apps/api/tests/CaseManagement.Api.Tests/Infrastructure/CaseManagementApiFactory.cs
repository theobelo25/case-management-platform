using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CaseManagement.Api.Tests.Infrastructure;

/// <summary>
/// Boots the API with test configuration. Set <see cref="ConnectionString"/> to a PostgreSQL database
/// (e.g. from Testcontainers) before the host is built.
/// </summary>
public sealed class CaseManagementApiFactory : WebApplicationFactory<Program>
{
    public required string ConnectionString { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        foreach (var pair in TestAppConfiguration.Build(ConnectionString))
        {
            if (pair.Value is not null)
                builder.UseSetting(pair.Key, pair.Value);
        }

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(TestAppConfiguration.Build(ConnectionString));
        });
    }
}

internal static class TestAppConfiguration
{
    internal static IReadOnlyDictionary<string, string?> Build(string connectionString) =>
        new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString,
            ["Jwt:Issuer"] = "CaseManagementTests",
            ["Jwt:Audience"] = "CaseManagementTests",
            ["Jwt:SigningKey"] = new string('t', 64),
            ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
            ["AllowedHosts"] = "*",
            ["ForwardedHeaders:Enabled"] = "false",
            ["Hsts:Enabled"] = "false",
            ["RateLimiting:AuthPermitLimit"] = "10000",
            ["RateLimiting:AuthWindowSeconds"] = "60",
            ["RateLimiting:CasesPermitLimit"] = "10000",
            ["RateLimiting:CasesWindowSeconds"] = "60"
        };
}
