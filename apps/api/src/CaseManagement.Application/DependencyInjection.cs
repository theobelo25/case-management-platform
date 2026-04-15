using CaseManagement.Application.Auth;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Organizations;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Ports;
using CaseManagement.Application.Users;
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
        services.AddScoped<IOrganizationPolicies, OrganizationPolicies>();
        services.AddScoped<IOrganizationsService, OrganizationsService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<ICasesService, CasesService>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
} 