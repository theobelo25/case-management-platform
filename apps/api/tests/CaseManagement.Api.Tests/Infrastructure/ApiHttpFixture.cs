using CaseManagement.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace CaseManagement.Api.Tests.Infrastructure;

/// <summary>
/// PostgreSQL via Testcontainers (Docker). When Docker is unavailable, <see cref="UnavailableReason"/> is set and HTTP tests should skip.
/// </summary>
public sealed class ApiHttpFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;

    public CaseManagementApiFactory? Factory { get; private set; }
    public HttpClient? Client { get; private set; }
    public string? UnavailableReason { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();

            await _postgres.StartAsync();

            var factory = new CaseManagementApiFactory
            {
                ConnectionString = _postgres.GetConnectionString()
            };
            Factory = factory;

            Client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CaseManagementDbContext>();
            await db.Database.MigrateAsync();
        }
        catch (DockerUnavailableException ex)
        {
            UnavailableReason = ex.Message;
        }
    }

    public async Task DisposeAsync()
    {
        if (Factory is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
            Factory?.Dispose();

        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }
}
