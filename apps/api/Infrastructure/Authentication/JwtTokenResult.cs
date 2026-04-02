namespace CaseManagement.Api.Infrastructure.Authentication;

public sealed class JwtTokenResult
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}