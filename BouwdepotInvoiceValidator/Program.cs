using BouwdepotInvoiceValidator.Domain.Services;
using Microsoft.Extensions.DependencyInjection; // Add this for ServiceCollectionExtensions
using Microsoft.OpenApi.Models;
using Serilog; // Added for Serilog

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

    builder.Services.AddDocumentValidation();
    // --- Register Gemini Client ---
    builder.Services.AddGemini(options =>
    {
        builder.Configuration.GetSection("Gemini").Bind(options);
    });

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
