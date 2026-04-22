using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace CaseManagement.Api.Configuration;

internal static class ForwardedHeadersConfiguration
{
    /// <summary>
    /// Applies <see cref="ForwardedHeadersSettings"/> to framework options. Safe to call when settings are null (no-op).
    /// </summary>
    internal static void Apply(ForwardedHeadersSettings? settings, ForwardedHeadersOptions options)
    {
        if (settings is null || !settings.Enabled)
            return;

        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        if (settings.IncludeHostHeader)
            options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;

        // Default chain depth for common setups (e.g. CDN → ingress → app). Override via config when needed.
        options.ForwardLimit = settings.ForwardLimit ?? 2;

        if (settings.ReplaceKnownNetworksAndProxies)
        {
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        }

        foreach (var proxy in settings.KnownProxies)
        {
            if (IPAddress.TryParse(proxy.Trim(), out var ip))
                options.KnownProxies.Add(ip);
        }

        foreach (var network in settings.KnownNetworks)
        {
            if (string.IsNullOrWhiteSpace(network.Prefix))
                continue;

            var cidr = $"{network.Prefix.Trim()}/{network.PrefixLength}";
            if (System.Net.IPNetwork.TryParse(cidr, out var net))
                options.KnownIPNetworks.Add(net);
        }
    }
}
