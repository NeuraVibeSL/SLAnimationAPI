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

app.MapGet("/ping", () => "API is running!");

// Appliquer la politique CORS
app.UseCors("AllowAllOrigins");

app.UseRouting();

app.MapControllers();

app.Run();

Console.WriteLine($"Application running on: {string.Join(", ", builder.WebHost.GetSetting("applicationUrl"))}");
