namespace CaseManagement.Api.Auth;

public sealed class RefreshTokenCookieService(IWebHostEnvironment environment) : IRefreshTokenCookieService
{
    public bool TryGetRefreshToken(HttpRequest request, out string? refreshToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Cookies.TryGetValue(RefreshTokenCookie.Name, out refreshToken);
    }
    
    public void Append(HttpResponse response, string refreshToken, DateTimeOffset expiresAtUtc)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = expiresAtUtc 
        };

        response.Cookies.Append(RefreshTokenCookie.Name, refreshToken, options);
    }

    public void Delete(HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        response.Cookies.Delete(RefreshTokenCookie.Name, options);
    }
}