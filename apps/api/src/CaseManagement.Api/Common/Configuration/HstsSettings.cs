namespace CaseManagement.Api.Configuration;

/// <summary>
/// Binds <c>Hsts:*</c>. When <see cref="Enabled"/> is true, the API sends <c>Strict-Transport-Security</c>
/// for HTTPS responses (see pipeline <c>UseHsts</c>). Preload should stay off unless you intentionally submit
/// the domain to the browser preload list and meet the minimum max-age requirements.
/// </summary>
public sealed class HstsSettings
{
    public const string SectionName = "Hsts";

    /// <summary>
    /// Registers HSTS middleware. Prefer false in Development; true in Production for HTTPS-only APIs.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Duration clients should remember HTTPS-only for this host. One year is a common baseline; preload eligibility
    /// typically expects at least ~18 weeks (see browser preload program rules).
    /// </summary>
    public int MaxAgeDays { get; set; } = 365;

    public bool IncludeSubDomains { get; set; } = true;

    /// <summary>
    /// Opt-in for browser HSTS preload lists. Only enable with a deliberate process and suitable max-age.
    /// </summary>
    public bool Preload { get; set; }

    /// <summary>
    /// Hostnames that must not receive HSTS (e.g. local test hosts if ever hit via HTTPS).
    /// </summary>
    public string[] ExcludedHosts { get; set; } = [];
}
