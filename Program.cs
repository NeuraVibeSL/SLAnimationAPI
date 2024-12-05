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
builder.Services.AddLogging();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Appliquer la politique CORS
app.UseCors("AllowAllOrigins");

app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unhandled exception: {ex.Message}");
        throw;
    }
});

// Middleware pour ajuster les en-têtes
app.Use(async (context, next) =>
{
    if (context.Request.Method == "GET")
    {
        context.Request.Headers.Remove("Content-Type");
    }
    await next.Invoke();
});

// Tester l'API avec un endpoint simple
app.MapGet("/ping", () => "API is running!");

// Activer le routage
app.UseRouting();

// Activer les contrôleurs
app.MapControllers();

app.Run();

Console.WriteLine("Application running.");

