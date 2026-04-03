namespace CaseManagement.Application.Auth.Options;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "Jwt:RefreshToken";

    public string CookieName { get; set; } = "refresh_token";
    public int ExpiryDays { get; set; } = 14;
    public string CookiePath { get; set; } = "/auth";
}
