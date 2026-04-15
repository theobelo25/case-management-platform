using System.Text;
using CaseManagement.Api.Auth;
using CaseManagement.Api.Exceptions;
using CaseManagement.Api.Validators;
using CaseManagement.Api.Configuration;
using CaseManagement.Application;
using CaseManagement.Infrastructure;
using CaseManagement.Infrastructure.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CaseManagement.Infrastructure.Persistence;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CaseManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "FrontendCorsPolicy";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<AuthRequestMarker>();
        
        services.AddEndpointsApiExplorer();
        
        const string bearerSchemeId = "Bearer"; // or "bearer" if you prefer lowercase everywhere

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(bearerSchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT Authorization header using the Bearer scheme. " +
                    "Example: \"Authorization: Bearer {token}\""
            });
            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(bearerSchemeId, document)] = []
                });
        });
        services.AddOpenApi();

        services.AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>();

        services.AddApplication();
        services.AddInfrastructure(configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<CaseManagementDbContext>();

        var rateLimiting = configuration.GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? httpContext.Connection.Id;
                return RateLimitPartition.GetFixedWindowLimiter(
                    ip,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimiting.AuthPermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimiting.AuthWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
            options.AddPolicy("cases", httpContext =>
            {
                var partitionKey = httpContext.User.GetUserIdOrNull() is Guid userId
                    ? $"user:{userId:N}"
                    : httpContext.Connection.RemoteIpAddress?.ToString()
                      ?? httpContext.Connection.Id;
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimiting.CasesPermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimiting.CasesWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
            options.OnRejected = OnAuthRateLimitRejected;
        });

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is missing.");
        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
            throw new InvalidOperationException(
                "Jwt:SigningKey is not configured. For local development use: dotnet user-secrets set \"Jwt:SigningKey\" \"<key>\" --project CaseManagement.Api.csproj. In other environments use environment variables or a secret store.");

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
            });
        
        services.AddAuthorization();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        var cors = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? throw new InvalidOperationException("Cors configuration is missing.");
        
        if (cors.AllowedOrigins.Length == 0)
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
        
        services.AddCors(options =>
            options.AddPolicy(FrontendCorsPolicy, policy =>
                policy.WithOrigins(cors.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

        return services;
    }

    private static async ValueTask OnAuthRateLimitRejected(
        OnRejectedContext context,
        CancellationToken ct = default)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.", ct);
    }
}

