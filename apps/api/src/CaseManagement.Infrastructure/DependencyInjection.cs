using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Infrastructure.Auth;
using CaseManagement.Infrastructure.Persistence;
using CaseManagement.Infrastructure.Persistence.Queries;
using CaseManagement.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddDbContextCheck<CaseManagementDbContext>("database", tags: ["ready"]);

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");
        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
            throw new InvalidOperationException(
                "JWT signing key (Jwt:SigningKey) is not configured. For local development use: dotnet user-secrets set \"Jwt:SigningKey\" \"<key>\" --project CaseManagement.Api.csproj. In other environments use environment variables or a secret store.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // WebSockets cannot send Authorization; SignalR clients pass the JWT via ?access_token=...
                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        if (!path.StartsWithSegments("/hubs"))
                            return Task.CompletedTask;

                        var accessToken = context.Request.Query["access_token"].ToString();
                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });
        
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IRefreshTokenHasher, Sha256RefreshTokenHasher>();
        services.AddSingleton<IRefreshTokenFactory, CryptoRefreshTokenFactory>();
        
        services.AddScoped<IRefreshTokenPersistence, EfRefreshTokenPersistence>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<UserRepository>();
        services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());
        services.AddScoped<IUserDisplayNameLookup>(sp => sp.GetRequiredService<UserRepository>());
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CaseManagementDbContext>());
        services.AddScoped<IOrganizationReadRepository, OrganizationReadRepository>();
        services.AddScoped<IOrganizationMembershipRepository, OrganizationMembershipRepository>();
        services.AddScoped<IOrganizationManagementRepository, OrganizationManagementRepository>();
        services.AddScoped<ICaseRepository, CaseRepository>();

        services.AddScoped<IUserOrganizationMembershipsQuery, UserOrganizationMembershipsQuery>();
        services.AddScoped<IOrganizationDetailQuery, OrganizationDetailQuery>();
        services.AddScoped<IUsersSearchQuery, UsersSearchQuery>();
        services.AddSingleton<ICaseListCursorCodec, CaseListCursorCodec>();
        services.AddScoped<ICaseListFilterApplier, CaseListFilterApplier>();
        services.AddScoped<ICaseListSortStrategy, UpdatedAtCaseListSortStrategy>();
        services.AddScoped<ICaseListSortStrategy, PriorityCaseListSortStrategy>();
        services.AddScoped<ICaseListSortStrategy, SlaDueCaseListSortStrategy>();
        services.AddScoped<ICaseListSortStrategyResolver, CaseListSortStrategyResolver>();
        services.AddScoped<ICaseListQuery, CaseListQuery>();
        services.AddScoped<ICaseVolumeOverTimeQuery, CaseVolumeOverTimeQuery>();
        services.AddScoped<IFirstResponseTimeOverTimeQuery, FirstResponseTimeOverTimeQuery>();
        services.AddScoped<ICaseStatusCountsQuery, CaseStatusCountsQuery>();
        services.AddScoped<IOrganizationPrivilegedUserIdsQuery, OrganizationPrivilegedUserIdsQuery>();
        services.AddScoped<IDueSoonCasesStore, DueSoonCasesStore>();

        return services;
    }
}