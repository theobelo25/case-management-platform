using CaseManagement.Api.Configuration;
using CaseManagement.Api.Middleware;
using CaseManagement.Api.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CaseManagement.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // Outermost: keep TraceId scope active while the exception handler runs.
        app.UseMiddleware<RequestLoggingScopeMiddleware>();

        app.UseExceptionHandler();

        // Must run before HTTPS redirection, rate limiting, and anything that uses RemoteIpAddress / scheme.
        var forwardedHeaders = app.Configuration.GetSection(ForwardedHeadersSettings.SectionName)
            .Get<ForwardedHeadersSettings>();
        if (forwardedHeaders?.Enabled == true)
            app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            // OpenAPI document must stay reachable without a JWT (same idea as Swagger middleware below).
            app.MapOpenApi().AllowAnonymous();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var hasConfiguredHttpsPort = !string.IsNullOrWhiteSpace(app.Configuration["HTTPS_PORT"])
            || !string.IsNullOrWhiteSpace(app.Configuration["ASPNETCORE_HTTPS_PORT"]);
        var hasHttpsUrl = (app.Configuration["ASPNETCORE_URLS"] ?? app.Configuration["urls"] ?? string.Empty)
            .Contains("https://", StringComparison.OrdinalIgnoreCase);

        var hsts = app.Configuration.GetSection(HstsSettings.SectionName).Get<HstsSettings>();
        if (hsts?.Enabled == true)
            app.UseHsts();

        if (hasConfiguredHttpsPort || hasHttpsUrl || !app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        // Place CORS before authentication/authorization so preflight requests succeed.
        app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicy);

        app.UseRateLimiter();
        
        app.UseAuthentication();
        app.UseAuthorization();

        // See repository README: liveness vs readiness and what each endpoint evaluates.
        app.MapHealthChecks("/api/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        }).AllowAnonymous();

        app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready")
        }).AllowAnonymous();

        app.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            Predicate = _ => true
        }).AllowAnonymous();
        app.MapControllers();
        app.MapHub<NotificationsHub>("/hubs/notifications");

        return app;
    }
}