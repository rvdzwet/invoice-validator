using BouwdepotInvoiceValidator.Domain.Services; // Corrected namespace for AddDocumentValidation
using BouwdepotInvoiceValidator.Infrastructure.Providers.Google; // For Gemini services
using BouwdepotInvoiceValidator.Infrastructure.Ollama; // Corrected namespace for Ollama services
using Microsoft.OpenApi.Models;
using Serilog;
using BouwdepotInvoiceValidator.Infrastructure.Google;

var builder = WebApplication.CreateBuilder(args); // Moved builder creation up

// Configure Serilog logger first using the created builder
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read configuration from appsettings.json
    .Enrich.FromLogContext()
    .CreateBootstrapLogger(); // Use bootstrap logger until host is built

try
{
    Log.Information("Starting web application");

    // Configure Serilog for the host
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // Read full config once host is built
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    // Add API controllers
    builder.Services.AddControllers();

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Bouwdepot Invoice Validator API",
            Version = "v1",
            Description = "API for validating invoices related to home improvement expenses"
        });
    });

    builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();

    // Register domain services (depends on ILLMProvider being registered below)
    builder.Services.AddDocumentValidation();

    // --- Conditionally Register LLM Provider ---
    string? llmProvider = builder.Configuration.GetValue<string>("LlmProvider");
    Log.Information("Configured LLM Provider: {LlmProvider}", llmProvider);

    switch (llmProvider?.ToLowerInvariant())
    {
        case "ollama":
            Log.Information("Registering Ollama provider...");
            builder.Services.AddOllamaProvider(builder.Configuration);
            break;
        // Removed duplicate case "gemini":
        case "gemini":
        default: // Default to Gemini if not specified or invalid
            Log.Information("Registering Gemini provider...");
            // Reverting to the original AddGemini registration method found earlier
            builder.Services.AddGemini(options =>
            {
                builder.Configuration.GetSection("Gemini").Bind(options);
            });
            break;
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseHttpsRedirection();
        // CORS is needed if the API is consumed by other origins, 
        // but not strictly required when serving the React app from the same origin.
        // Keep AllowAll for development flexibility.
    }
    else
    {
        // In development, use the more permissive CORS policy
        app.UseCors("AllowAll");
    }

    // Configure Swagger
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bouwdepot Invoice Validator API v1"));
    }

    // Serve static files from wwwroot (where the React app is built)
    app.UseStaticFiles();

    // Map API controllers first
    app.MapControllers();

    // Fallback to index.html for client-side routing
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are flushed on shutdown
}
