namespace CaseManagement.Application.Auth;

public sealed class JwtTokenResult
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}