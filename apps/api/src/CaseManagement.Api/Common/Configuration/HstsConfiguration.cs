using Microsoft.AspNetCore.HttpsPolicy;

namespace CaseManagement.Api.Configuration;

internal static class HstsConfiguration
{
    internal static void Apply(HstsSettings? settings, HstsOptions options)
    {
        if (settings is null || !settings.Enabled)
            return;

        options.MaxAge = TimeSpan.FromDays(Math.Max(1, settings.MaxAgeDays));
        options.IncludeSubDomains = settings.IncludeSubDomains;
        options.Preload = settings.Preload;

        foreach (var host in settings.ExcludedHosts)
        {
            if (!string.IsNullOrWhiteSpace(host))
                options.ExcludedHosts.Add(host.Trim());
        }
    }
}
