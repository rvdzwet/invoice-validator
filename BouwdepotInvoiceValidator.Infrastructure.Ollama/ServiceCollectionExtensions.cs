using BouwdepotValidationValidator.Infrastructure.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BouwdepotInvoiceValidator.Infrastructure.Ollama
{
    /// <summary>
    /// Extension methods for setting up Ollama services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Ollama services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configuration">The configuration containing Ollama settings.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddOllamaProvider(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure OllamaOptions from the "Ollama" section
            services.AddOptions<OllamaOptions>()
                .Bind(configuration.GetSection(OllamaOptions.SectionName))
                .ValidateDataAnnotations() // Add validation based on annotations in OllamaOptions
                .ValidateOnStart(); // Validate options on application startup

            // Register HttpClient for OllamaClient
            // Using AddHttpClient ensures proper management of HttpClient instances
            services.AddHttpClient<ILLMProvider, OllamaClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.ApiBaseUrl) && Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var baseUri))
                {
                    client.BaseAddress = baseUri;
                }
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });
            // Note: We register OllamaClient as the implementation for ILLMProvider here.
            // In Program.cs, we will conditionally call either AddGeminiProvider or AddOllamaProvider.

            return services;
        }
    }
}
