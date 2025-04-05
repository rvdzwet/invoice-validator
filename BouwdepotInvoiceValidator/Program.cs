using BouwdepotInvoiceValidator.Services;
using BouwdepotInvoiceValidator.Services.AI;
using BouwdepotInvoiceValidator.Services.Security;
using BouwdepotInvoiceValidator.Services.Vendors;
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

// Register application services
builder.Services.AddScoped<IPdfExtractionService, PdfExtractionService>();

// Register AI service abstractions
builder.Services.AddScoped<IAIService, GeminiAIService>();
builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();

// GeminiServiceBase is abstract and cannot be registered directly

// Register Gemini specialized services in the proper order
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiConversationService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiDocumentAnalysisService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiHomeImprovementService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiFraudDetectionService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiLineItemAnalysisService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiAdvancedAnalysisService>();

// Register the main service implementation
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();

// Register GeminiServiceBase using GeminiConversationService as the implementation
// This is needed for GeminiModelProvider
builder.Services.AddScoped<GeminiServiceBase>(sp => 
    sp.GetRequiredService<BouwdepotInvoiceValidator.Services.Gemini.GeminiConversationService>());

// Register legacy services for backward compatibility
builder.Services.AddScoped<GeminiDocumentService>();
builder.Services.AddScoped<GeminiHomeImprovementService>();
builder.Services.AddScoped<GeminiAdvancedAnalysisService>();
builder.Services.AddScoped<GeminiLineItemAnalysisService>();
builder.Services.AddScoped<GeminiServiceProxy>();

// Register Bouwdepot validation services
builder.Services.AddScoped<IBouwdepotRulesValidationService, BouwdepotRulesValidationService>();
builder.Services.AddScoped<IAuditReportService, AuditReportService>();

// Register security services
builder.Services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();

// Register vendor profiling services
builder.Services.AddSingleton<IVendorRepository, InMemoryVendorRepository>(); // Singleton for in-memory repository
builder.Services.AddScoped<IVendorProfileService, VendorProfileService>();

// Register AI model provider services
builder.Services.AddScoped<AIModelProviderFactory>();
builder.Services.AddScoped<GeminiModelProvider>();
builder.Services.AddSingleton<ImageProcessingService>();
// GeminiImageGenerator is now created within GeminiAIService with the proper logger
builder.Services.AddScoped<IAIDecisionExplainer, AIDecisionExplainer>();

// Register prompt template services
builder.Services.AddSingleton<BouwdepotInvoiceValidator.Services.Prompts.PromptFileService>();
builder.Services.AddSingleton<BouwdepotInvoiceValidator.Services.Prompts.PromptTemplateService>();
builder.Services.AddHostedService<BouwdepotInvoiceValidator.Services.Prompts.PromptInitializationService>();

// Use the unified validation service
builder.Services.AddScoped<IInvoiceValidationService, UnifiedInvoiceValidationService>();

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

// Removed Antiforgery as it's typically used with Razor Pages/MVC forms, not SPA APIs
// app.UseAntiforgery(); 

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
