using CaseManagement.Application.Auth.Options;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Database;
using CaseManagement.Infrastructure.Authentication;
using CaseManagement.Infrastructure.Configuration;
using CaseManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CaseManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(
            configuration.GetSection(DatabaseOptions.SectionName));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddSingleton<IValidateOptions<RefreshTokenOptions>, RefreshTokenOptionsValidator>();

        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Database configuration is missing.");

        if (string.IsNullOrWhiteSpace(databaseOptions.ConnectionString))
            throw new InvalidOperationException("Database connection string is missing.");

        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(databaseOptions.ConnectionString));
            
        services.AddScoped<IDevelopmentDatabaseInitializer, DevelopmentDatabaseInitializer>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<IRefreshTokenIssuer, RefreshTokenIssuer>();
        services.AddScoped<IRefreshTokenValidator, RefreshTokenValidator>();
        
        services
            .AddHealthChecks()
            .AddNpgSql(databaseOptions.ConnectionString, name: "postgres");
        return services;
    }
}
