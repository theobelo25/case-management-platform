namespace CaseManagement.Application.Auth;
public sealed record AuthResult(
    string AccessToken, 
    string RefreshToken, 
    DateTimeOffset RefreshTokenExpiresAtUtc);