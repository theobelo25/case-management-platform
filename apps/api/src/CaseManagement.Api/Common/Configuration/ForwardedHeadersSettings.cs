namespace CaseManagement.Api.Configuration;

/// <summary>
/// Binds <c>ForwardedHeaders:*</c>. Used with forwarded-headers middleware so client IP and request scheme
/// match the caller when the app sits behind nginx, Azure App Service, Cloudflare, etc.
/// </summary>
public sealed class ForwardedHeadersSettings
{
    public const string SectionName = "ForwardedHeaders";

    /// <summary>
    /// When true, registers <c>UseForwardedHeaders</c> and applies the options below. Disable for local Kestrel without a proxy.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Max number of entries to take from the end of X-Forwarded-For / X-Forwarded-Proto chains.
    /// Use 2 or more when traffic passes through multiple proxies (e.g. Cloudflare then nginx).
    /// </summary>
    public int? ForwardLimit { get; set; }

    /// <summary>
    /// Process X-Forwarded-Host (useful for redirects and absolute URLs behind a reverse proxy).
    /// </summary>
    public bool IncludeHostHeader { get; set; }

    /// <summary>
    /// When true, clears the default known private networks and loopback proxies before applying
    /// <see cref="KnownNetworks"/> and <see cref="KnownProxies"/>. Use only with a complete allowlist.
    /// </summary>
    public bool ReplaceKnownNetworksAndProxies { get; set; }

    public string[] KnownProxies { get; set; } = [];

    public ForwardedNetworkEntry[] KnownNetworks { get; set; } = [];
}

public sealed class ForwardedNetworkEntry
{
    public string Prefix { get; set; } = "";
    public int PrefixLength { get; set; }
}
