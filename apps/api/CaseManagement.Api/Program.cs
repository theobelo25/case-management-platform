using CaseManagement.Api;
using CaseManagement.Application;
using CaseManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWeb(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtBearerAuthentication();

var app = builder.Build();

await app.InitializeDatabaseInDevelopmentAsync();

app.UseWebPipeline();

app.Run();
