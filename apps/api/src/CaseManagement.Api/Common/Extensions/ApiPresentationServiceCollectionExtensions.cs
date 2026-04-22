using System.Text.Json;
using System.Text.Json.Serialization;
using CaseManagement.Api.Auth;
using CaseManagement.Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace CaseManagement.Api.Extensions;

internal static class ApiPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddApiPresentationServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<AuthRequestMarker>();

        return services;
    }
}
