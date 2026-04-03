using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace CaseManagement.Api;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }
}
