using BouwdepotInvoiceValidator.Infrastructure.Gemini;
using BouwdepotInvoiceValidator.Infrastructure.Gemini.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http; 
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Gemini client services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the Gemini client services using configuration from an IConfiguration section.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="configurationSectionName">The name of the configuration section to bind options from. Defaults to "Gemini".</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services or configuration is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the configuration section is missing or invalid.</exception>
        public static IServiceCollection AddGeminiClient(
            this IServiceCollection services,
            IConfiguration configuration,
            string configurationSectionName = "Gemini")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(configurationSectionName)) throw new ArgumentNullException(nameof(configurationSectionName));

            // Bind GeminiOptions from the specified configuration section
            services.Configure<GeminiOptions>(configuration.GetSection(configurationSectionName));

            // Validate options during startup
            services.AddOptions<GeminiOptions>()
                .Bind(configuration.GetSection(configurationSectionName))
                .ValidateDataAnnotations()
                .Validate(options =>
                {
                    // Add custom validation logic here if needed
                    if (string.IsNullOrWhiteSpace(options.ApiKey))
                    {
                        return false; // Example: API Key is required
                    }
                    if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out _))
                    {
                        return false; // Example: Base URL must be valid
                    }
                    return true;
                }, $"GeminiOptions failed validation. Ensure '{configurationSectionName}' section in configuration is valid.");

            // Register the HttpClient and the GeminiClient itself
            AddGeminiHttpClient(services);

            return services;
        }

        /// <summary>
        /// Registers the Gemini client services using a configuration action.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureOptions">An action to configure the <see cref="GeminiOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services or configureOptions is null.</exception>
        public static IServiceCollection AddGeminiClient(
            this IServiceCollection services,
            Action<GeminiOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            // Configure GeminiOptions using the provided action
            services.Configure(configureOptions);

            // Validate options during startup
            services.AddOptions<GeminiOptions>()
                .Configure(configureOptions)
                .ValidateDataAnnotations()
                .Validate(options =>
                {
                    // Add custom validation logic here if needed
                    if (string.IsNullOrWhiteSpace(options.ApiKey)) return false;
                    if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out _)) return false;
                    return true;
                }, "GeminiOptions failed validation. Ensure options configured via action are valid.");

            // Register the HttpClient and the GeminiClient itself
            AddGeminiHttpClient(services);

            return services;
        }

        private static void AddGeminiHttpClient(IServiceCollection services)
        {
            services.AddHttpClient<IGeminiClient, GeminiClient>((serviceProvider, client) =>
            {
                // Get configured options
                var geminiOptions = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;

                if (string.IsNullOrWhiteSpace(geminiOptions.ApiBaseUrl) || !Uri.TryCreate(geminiOptions.ApiBaseUrl, UriKind.Absolute, out var baseUri))
                {
                    throw new InvalidOperationException("Gemini API Base URL is missing or invalid in configuration.");
                }
                if (string.IsNullOrWhiteSpace(geminiOptions.ApiKey))
                {
                    throw new InvalidOperationException("Gemini API Key is missing in configuration.");
                }

                client.BaseAddress = baseUri;
                client.Timeout = TimeSpan.FromSeconds(geminiOptions.TimeoutSeconds > 0 ? geminiOptions.TimeoutSeconds : 30); // Default timeout
                client.DefaultRequestHeaders.Add("x-goog-api-key", geminiOptions.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(GetRetryPolicy); // Add Polly retry policy
                                               // You could add more handlers here, like circuit breakers if needed
                                               // .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        /// <summary>
        /// Defines a basic retry policy for handling transient HTTP errors.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider, HttpRequestMessage request)
        {
            // Get configured options to determine max retries
            // Note: Resolving options directly here can be tricky if options change.
            // Usually, policies are configured once. If dynamic options are needed per request,
            // it requires more complex Polly context usage.
            // For simplicity, we resolve options once when the policy is created.
            var geminiOptions = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;
            var maxRetries = geminiOptions.MaxRetries > 0 ? geminiOptions.MaxRetries : 3; // Default retries

            // Use Polly extensions for common transient error handling
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException, 5xx, 408
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Optionally retry on 429
                .WaitAndRetryAsync(
                    retryCount: maxRetries,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential back-off: 2s, 4s, 8s,...
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        // Optional: Log retries using ILogger if needed (requires injecting ILoggerFactory or getting logger from context)
                        var logger = serviceProvider.GetService<ILogger<GeminiClient>>(); // Or a more general logger
                        logger?.LogWarning("Delaying for {Timespan}ms, then making retry {RetryAttempt} of {MaxRetries} for request {RequestUri} due to {StatusCode}...",
                            timespan.TotalMilliseconds,
                            retryAttempt,
                            maxRetries,
                            outcome.Result?.RequestMessage?.RequestUri ?? request.RequestUri,
                            outcome.Result?.StatusCode);
                    }
                );
        }
    }
}