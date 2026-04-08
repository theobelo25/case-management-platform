namespace CaseManagement.Application;

using CaseManagement.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
} 