var builder = WebApplication.CreateBuilder(args);

// Configurer les services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Configurer les logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Construire l'application
var app = builder.Build();

// Appliquer la politique CORS
app.UseCors();

// Middleware global pour capturer les exceptions non gérées
app.Use(async (context, next) =>
{
    try
    {
        if (context.Request.Method == "GET")
        {
            context.Request.Headers.Remove("Content-Type"); // Optionnel
        }
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    }
});

// Tester l'API avec un endpoint simple
app.MapGet("/ping", () => "API is running!");

// Activer le routage
app.UseRouting();

// Activer les contrôleurs
app.MapControllers();

// Démarrer l'application
app.Run();
