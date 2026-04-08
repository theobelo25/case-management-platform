namespace CaseManagement.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Place CORS before authentication/authorization so preflight requests succeed.
        app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicy);

        app.UseRateLimiter();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/api/health");
        app.MapControllers();

        return app;
    }
}