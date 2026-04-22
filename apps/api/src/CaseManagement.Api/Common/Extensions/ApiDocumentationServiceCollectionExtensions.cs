using Microsoft.OpenApi;

namespace CaseManagement.Api.Extensions;

internal static class ApiDocumentationServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocumentationServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        const string bearerSchemeId = "Bearer";

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(bearerSchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT Authorization header using the Bearer scheme. " +
                    "Example: \"Authorization: Bearer {token}\""
            });
            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(bearerSchemeId, document)] = []
                });
        });

        services.AddOpenApi();

        return services;
    }
}
