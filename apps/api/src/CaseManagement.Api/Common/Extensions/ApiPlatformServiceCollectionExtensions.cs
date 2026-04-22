using System.Threading.RateLimiting;
using CaseManagement.Api.Configuration;
using CaseManagement.Api.Exceptions;
using CaseManagement.Api.Exceptions.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagement.Api.Extensions;

internal static class ApiPlatformServiceCollectionExtensions
{
    public static IServiceCollection AddApiPlatformServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimiting = configuration.GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            var settings = configuration.GetSection(ForwardedHeadersSettings.SectionName)
                .Get<ForwardedHeadersSettings>();
            ForwardedHeadersConfiguration.Apply(settings, options);
        });

        services.Configure<HstsOptions>(options =>
        {
            var settings = configuration.GetSection(HstsSettings.SectionName).Get<HstsSettings>();
            HstsConfiguration.Apply(settings, options);
        });

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
            options.OnRejected = OnRateLimitRejected;
        });

        services.AddAuthorization(AuthorizationPolicies.ConfigureDefault);
        services.AddProblemDetails();

        services.AddSingleton<ExceptionProblemDetailsWriter>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, ValidationExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, ConflictExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, AuthenticationFailedExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, NotFoundExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, InvalidPasswordExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, BadRequestArgumentExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, ForbiddenExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper, ArgumentExceptionMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapperRegistry, ExceptionToProblemDetailsMapperRegistry>();
        services.AddSingleton<DevelopmentDatabaseExceptionResponder>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddFrontendCorsPolicy(configuration);

        return services;
    }

    private static IServiceCollection AddFrontendCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cors = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? throw new InvalidOperationException("Cors configuration is missing.");

        if (cors.AllowedOrigins.Length == 0)
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");

        services.AddCors(options =>
            options.AddPolicy(ServiceCollectionExtensions.FrontendCorsPolicy, policy =>
                policy.WithOrigins(cors.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

        return services;
    }

    private static async ValueTask OnRateLimitRejected(
        OnRejectedContext context,
        CancellationToken cancellationToken = default)
    {
        var httpContext = context.HttpContext;
        int? retryAfterSeconds = null;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterSeconds = (int)retryAfter.TotalSeconds;
            httpContext.Response.Headers.RetryAfter = retryAfterSeconds.Value.ToString();
        }

        var writer = httpContext.RequestServices.GetRequiredService<ExceptionProblemDetailsWriter>();
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests",
            Detail = "The rate limit for this operation has been exceeded. Try again later.",
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["code"] = "rate_limit_exceeded";
        if (retryAfterSeconds is int seconds)
            problem.Extensions["retryAfterSeconds"] = seconds;

        await writer.WriteProblemAsync(httpContext, problem, cancellationToken);
    }
}
