using CaseManagement.Application.Auth;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Organizations;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Cases.Services;
using CaseManagement.Application.Cases.Services.BulkActions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using CaseServices = CaseManagement.Application.Cases.Services;

namespace CaseManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Action<ApplicationRegistrationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var options = new ApplicationRegistrationOptions();
        configure(options);

        options.RegisterCaseNotificationPublisher(services);
        options.RegisterOrganizationMembershipNotifier(services);

        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddScoped<IRegisterAuthHandler, RegisterAuthHandler>();
        services.AddScoped<ILoginAuthHandler, LoginAuthHandler>();
        services.AddScoped<IRefreshAuthHandler, RefreshAuthHandler>();
        services.AddScoped<ILogoutAuthHandler, LogoutAuthHandler>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        
        services.AddScoped<ICreateOrganizationService, CreateOrganizationService>();
        services.AddScoped<OrganizationPolicies>();
        services.AddScoped<IOrganizationPolicies>(sp => sp.GetRequiredService<OrganizationPolicies>());
        services.AddScoped<IOrganizationCaseManagementPolicy>(sp => sp.GetRequiredService<OrganizationPolicies>());
        services.AddScoped<IOrganizationsService, OrganizationsService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<CaseAccessResolver>();
        services.AddScoped<IBulkCaseActionHandler, BulkAssignCaseActionHandler>();
        services.AddScoped<IBulkCaseActionHandler, BulkSetPriorityCaseActionHandler>();
        services.AddScoped<IBulkCaseActionHandler, BulkSetStatusCaseActionHandler>();
        services.AddScoped<IBulkCaseActionHandler, BulkBumpPriorityCaseActionHandler>();
        services.AddScoped<CaseServices.ICaseBulkUpdateService, CaseBulkUpdateService>();
        services.AddScoped<CaseServices.ICaseCreationService, CaseCreationService>();
        services.AddScoped<CaseServices.ICaseQueryService, CaseQueryService>();
        services.AddScoped<CaseServices.ISlaBreachPostActionHandler, SlaBreachPostActionHandler>();
        services.AddScoped<CaseServices.ICaseUpdatedPostActionHandler, CaseUpdatedPostActionHandler>();
        services.AddScoped<CaseServices.ICaseCommentedPostActionHandler, CaseCommentedPostActionHandler>();
        services.AddScoped<CaseServices.ICaseAssignedPostActionHandler, CaseAssignedPostActionHandler>();
        services.AddScoped<CaseServices.ICaseWorkflowService, CaseWorkflowService>();
        services.AddScoped<CaseServices.ICaseAnalyticsService, CaseAnalyticsService>();
        services.AddScoped<CaseServices.IDueSoonCaseSelectionService, DueSoonCaseSelectionService>();
        services.AddScoped<CaseServices.IDueSoonRecipientResolver, DueSoonRecipientResolver>();
        services.AddScoped<CaseServices.IDueSoonNotificationDeduper, DueSoonNotificationDeduper>();
        services.AddScoped<CaseServices.IDueSoonNotificationDispatcher, DueSoonNotificationDispatcher>();
        services.AddScoped<CaseApplicationService>();
        services.AddScoped<ICaseCommandService>(sp => sp.GetRequiredService<CaseApplicationService>());
        services.AddScoped<CaseManagement.Application.Cases.Ports.ICaseQueryService>(
            sp => sp.GetRequiredService<CaseApplicationService>());
        services.AddScoped<CaseManagement.Application.Cases.Ports.ICaseAnalyticsService>(
            sp => sp.GetRequiredService<CaseApplicationService>());
        services.AddScoped<IDueSoonNotificationProcessor, DueSoonNotificationProcessor>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

public sealed class ApplicationRegistrationOptions
{
    internal Action<IServiceCollection> RegisterCaseNotificationPublisher { get; private set; } =
        services => services.AddSingleton<ICaseNotificationPublisher, NoOpCaseNotificationPublisher>();

    internal Action<IServiceCollection> RegisterOrganizationMembershipNotifier { get; private set; } =
        services => services.AddSingleton<IOrganizationMembershipNotifier, NoOpOrganizationMembershipNotifier>();

    /// <summary>
    /// Registers the realtime notification implementation (e.g. SignalR from the API host). Without this,
    /// <see cref="ICaseNotificationPublisher"/> resolves to a no-op implementation.
    /// </summary>
    public ApplicationRegistrationOptions UseCaseNotificationPublisher<TService>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : class, ICaseNotificationPublisher
    {
        RegisterCaseNotificationPublisher = services =>
            services.Add(new ServiceDescriptor(typeof(ICaseNotificationPublisher), typeof(TService), lifetime));
        return this;
    }

    public ApplicationRegistrationOptions UseOrganizationMembershipNotifier<TService>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : class, IOrganizationMembershipNotifier
    {
        RegisterOrganizationMembershipNotifier = services =>
            services.Add(new ServiceDescriptor(typeof(IOrganizationMembershipNotifier), typeof(TService), lifetime));
        return this;
    }
}