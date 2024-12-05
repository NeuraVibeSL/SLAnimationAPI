var builder = WebApplication.CreateBuilder(args);

// Configurer CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Appliquer la politique CORS
app.UseCors("AllowAllOrigins");

// Middleware pour modifier les en-têtes des requêtes GET
app.Use(async (context, next) =>
{
    if (context.Request.Method == "GET")
    {
        context.Request.Headers.Remove("Content-Type");
    }
    await next.Invoke();
});

app.MapGet("/ping", () => "API is running!");

app.UseRouting();

app.MapControllers();

app.Run();

Console.WriteLine($"Application running.");
