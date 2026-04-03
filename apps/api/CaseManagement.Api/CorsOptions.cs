namespace CaseManagement.Api;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>
    /// Browser origins allowed to call this API (e.g. Angular dev server). Empty means no cross-origin browser access.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}
