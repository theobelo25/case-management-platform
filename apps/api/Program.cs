var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Services
// -------------------------

// Controllers (needed for MapControllers)
builder.Services.AddControllers();

// OpenAPI (new minimal API style)
builder.Services.AddOpenApi();

// Swagger (UI + JSON)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (for Angular dev server)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// -------------------------
// Middleware / Pipeline
// -------------------------

if (app.Environment.IsDevelopment())
{
    // New OpenAPI endpoint
    app.MapOpenApi();

    // Swagger (JSON + UI)
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
