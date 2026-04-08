namespace CaseManagement.Api.Auth;

public interface IRefreshTokenCookieService
{
    bool TryGetRefreshToken(HttpRequest request, out string? refreshToken);
    void Append(HttpResponse response, string refreshToken, DateTimeOffset expiresAtUtc);
    void Delete(HttpResponse response);
}