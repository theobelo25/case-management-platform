using Microsoft.AspNetCore.Authorization;

namespace CaseManagement.Api.Configuration;

/// <summary>
/// Central authorization defaults. Team convention: require authentication for all endpoints unless
/// explicitly marked <see cref="AllowAnonymousAttribute"/> (auth flows, logout, health, OpenAPI in Development, etc.).
/// </summary>
internal static class AuthorizationPolicies
{
    internal static void ConfigureDefault(AuthorizationOptions options)
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }
}
