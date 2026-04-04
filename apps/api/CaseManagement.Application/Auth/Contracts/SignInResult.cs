namespace CaseManagement.Application.Auth;

public sealed record SignInResult(
    AuthResponse Auth,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
