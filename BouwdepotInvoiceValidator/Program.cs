using BouwdepotInvoiceValidator.Components;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services;
using BouwdepotInvoiceValidator.Services.AI;
using BouwdepotInvoiceValidator.Services.Security;
using BouwdepotInvoiceValidator.Services.Vendors;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API controllers
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
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

// Register base services
builder.Services.AddSingleton<GeminiServiceBase>();

// Register Gemini specialized services in the proper order
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiConversationService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiDocumentAnalysisService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiHomeImprovementService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiFraudDetectionService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiLineItemAnalysisService>();
builder.Services.AddScoped<BouwdepotInvoiceValidator.Services.Gemini.GeminiAdvancedAnalysisService>();

// Register the main service implementation
builder.Services.AddScoped<IGeminiService, GeminiService>();

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
builder.Services.AddSingleton<AIModelProviderFactory>();
builder.Services.AddSingleton<GeminiModelProvider>();
builder.Services.AddSingleton<ImageProcessingService>();
// GeminiImageGenerator is now created within GeminiAIService with the proper logger
builder.Services.AddScoped<IAIDecisionExplainer, AIDecisionExplainer>();

// Use the unified validation service
builder.Services.AddScoped<IInvoiceValidationService, UnifiedInvoiceValidationService>();

// Enable JavaScript interop for file inputs and other interactive components
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseCors("AllowReactApp");
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

app.Run();
