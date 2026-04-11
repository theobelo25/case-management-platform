using CaseManagement.Application.Auth;
using CaseManagement.Application.Ports;
using CaseManagement.Infrastructure.Auth;
using CaseManagement.Infrastructure.Persistence;
using CaseManagement.Infrastructure.Persistence.Queries;
using CaseManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. For local development use: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<connection string>\" --project CaseManagement.Api.csproj.");

        services.AddDbContext<CaseManagementDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IRefreshTokenHasher, Sha256RefreshTokenHasher>();
        services.AddSingleton<IRefreshTokenFactory, CryptoRefreshTokenFactory>();
        
        services.AddScoped<IRefreshTokenPersistence, EfRefreshTokenPersistence>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CaseManagementDbContext>());
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        
        services.AddScoped<IUserOrganizationMembershipsQuery, UserOrganizationMembershipsQuery>();
        services.AddScoped<IOrganizationDetailQuery, OrganizationDetailQuery>();
        
        return services;
    }
}