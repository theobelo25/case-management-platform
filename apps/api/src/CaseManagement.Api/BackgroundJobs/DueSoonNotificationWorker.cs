using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using Microsoft.Extensions.Options;

namespace CaseManagement.Api.BackgroundJobs;

public sealed class DueSoonNotificationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<DueSoonSchedulerOptions> options,
    ILogger<DueSoonNotificationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IDueSoonNotificationProcessor>();
                await processor.RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DueSoon worker iteration failed.");
            }

            var intervalSeconds = Math.Max(30, options.Value.PollIntervalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
