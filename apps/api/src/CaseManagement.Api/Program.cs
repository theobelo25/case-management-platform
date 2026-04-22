using CaseManagement.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.ApplyDatabaseMigrationAndDemoSeedAsync();

app.UseApiPipeline();

app.Run();