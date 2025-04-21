using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoiceValidation(this IServiceCollection services)
        {
            services.AddScoped<IInvoiceValidationService , InvoiceValidationService>();
            services.AddSingleton<PromptService>();
            // Configure the GhostscriptOptions using the Options pattern
            services.Configure<GhostscriptOptions>(opts =>
            {
                opts.ExecutablePath = "C:\\Program Files\\gs\\gs10.05.0\\bin\\gswin64c.exe";/* configuration.GetSection(GhostscriptOptions.Ghostscript)["ExecutablePath"];*/
            });

            // Register the GhostscriptPdfToImageConverter as a service
            services.AddScoped<IPdfToImageConverter, GhostscriptPdfToImageConverter>();

            return services;
        }
    }
}
