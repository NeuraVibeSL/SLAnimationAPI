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
        if (context.Request.Method == "GET")
        {
            context.Request.Headers.Remove("Content-Type");
        }
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unhandled exception: {ex.Message}");
        throw;
    }
});

// Tester l'API avec un endpoint simple
app.MapGet("/ping", () => "API is running!");

// Activer le routage
app.UseRouting();

// Activer les contrôleurs
app.MapControllers();

app.Run();

Console.WriteLine("Application running.");

