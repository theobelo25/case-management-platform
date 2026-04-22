using CaseManagement.Api.Configuration;

namespace CaseManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "FrontendCorsPolicy";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApiPresentationServices();
        services.AddApiDocumentationServices();
        services.AddApiCoreServices(configuration);
        services.AddApiPlatformServices(configuration);

        return services;
    }

}

