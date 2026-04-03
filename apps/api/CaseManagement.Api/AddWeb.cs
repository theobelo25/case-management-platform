using CaseManagement.Api.Middleware;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace CaseManagement.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeb(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection(CorsOptions.SectionName);
        services.AddOptions<CorsOptions>().Bind(corsSection);
        var corsOrigins = corsSection.Get<CorsOptions>()?.AllowedOrigins ?? [];

        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Validation.SignInRequestValidator>();
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                if (corsOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(corsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    policy
                        .SetIsOriginAllowed(_ => false)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static WebApplication UseWebPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
