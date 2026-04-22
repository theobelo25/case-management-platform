using CaseManagement.Api.Auth;
using CaseManagement.Api.BackgroundJobs;
using CaseManagement.Api.Configuration;
using CaseManagement.Api.Realtime;
using CaseManagement.Application;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagement.Api.Extensions;

internal static class ApiCoreServiceCollectionExtensions
{
    public static IServiceCollection AddApiCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>();

        services.AddApplication(options =>
        {
            // Replaces the application layer default (no-op) with SignalR delivery for this host.
            options
                .UseCaseNotificationPublisher<CaseNotificationSignalRNotifier>(ServiceLifetime.Scoped)
                .UseOrganizationMembershipNotifier<OrganizationMembershipSignalRNotifier>();
        });

        services.AddInfrastructure(configuration);
        services.AddHostedService<DueSoonNotificationWorker>();
        services.Configure<DueSoonSchedulerOptions>(
            configuration.GetSection(DueSoonSchedulerOptions.SectionName));

        return services;
    }
}
