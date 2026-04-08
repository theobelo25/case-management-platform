namespace CaseManagement.Api.Configuration;
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public int AuthPermitLimit { get; set; } = 20;
    public int AuthWindowSeconds { get; set; } = 60;
}