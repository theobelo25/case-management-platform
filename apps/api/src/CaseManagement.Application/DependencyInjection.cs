using CaseManagement.Application.Auth;
using CaseManagement.Application.Organizations;
using CaseManagement.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        
        services.AddScoped<ICreateOrganizationService, CreateOrganizationService>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
} 