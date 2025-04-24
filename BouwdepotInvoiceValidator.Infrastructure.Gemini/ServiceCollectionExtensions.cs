using BouwdepotValidationValidator.Infrastructure.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace BouwdepotInvoiceValidator.Infrastructure.Google
{
    /// <summary>
    /// Extension methods for setting up Gemini client services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the Gemini client services using a configuration action.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureOptions">An action to configure the <see cref="GeminiOptions"/>.
        /// This action is responsible for setting the necessary options, potentially by binding
        /// from configuration or setting values directly.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services or configureOptions is null.</exception>
        /// <exception cref="OptionsValidationException">Thrown at startup if the configured options fail validation.</exception>
        public static IServiceCollection AddGemini(
            this IServiceCollection services,
            Action<GeminiOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            // Configure and Validate options using the provided action.
            // The 'configureOptions' action itself handles how the options are populated (e.g., binding from IConfiguration).
            services.AddOptions<GeminiOptions>()
                .Configure(configureOptions) // Register the configuration action
                .ValidateDataAnnotations()    // Enable validation via attributes on GeminiOptions
                .Validate(options => // Add custom imperative validation
                {
                    if (string.IsNullOrWhiteSpace(options.ApiKey))
                    {
                        // Consider logging this specific failure if ILogger is available via DI
                        return false; // API Key is required
                    }
                    if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) || !Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out _))
                    {
                        // Consider logging this specific failure
                        return false; // Base URL must be a valid absolute URI
                    }
                    // Add any other custom validation rules here
                    return true;
                }, "GeminiOptions failed validation. Ensure the options provided via the configuration action are valid (e.g., ApiKey and ApiBaseUrl are set correctly)."); // Updated error message

            // Register the HttpClient and the GeminiClient itself
            AddGeminiHttpClient(services);

            return services;
        }

        private static void AddGeminiHttpClient(IServiceCollection services)
        {
            services.AddHttpClient<ILLMProvider, GeminiClient>((serviceProvider, client) =>
            {
                // Get configured and validated options. The IOptions infrastructure ensures
                // the options have passed validation defined in AddGeminiClient.
                // Use IOptionsMonitor if you need to react to configuration changes at runtime.
                var geminiOptions = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;

                // Re-check critical values defensively, although validation should prevent invalid states.
                if (!Uri.TryCreate(geminiOptions.ApiBaseUrl, UriKind.Absolute, out var baseUri))
                {
                    // This should ideally not happen if validation is set up correctly.
                    throw new InvalidOperationException("Gemini API Base URL is invalid despite passing validation. Check validation logic.");
                }
                if (string.IsNullOrWhiteSpace(geminiOptions.ApiKey))
                {
                    // This should ideally not happen if validation is set up correctly.
                    throw new InvalidOperationException("Gemini API Key is missing despite passing validation. Check validation logic.");
                }

                client.BaseAddress = baseUri;
                client.Timeout = TimeSpan.FromSeconds(geminiOptions.TimeoutSeconds > 0 ? geminiOptions.TimeoutSeconds : 30); // Default timeout 30s
                client.DefaultRequestHeaders.Add("x-goog-api-key", geminiOptions.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                // Add other default headers if needed
            })
            // Add Polly retry policy using a factory that accesses IServiceProvider
            .AddPolicyHandler((serviceProvider, request) => GetRetryPolicy(serviceProvider, request));
        }

        /// <summary>
        /// Defines a basic retry policy for handling transient HTTP errors.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider, HttpRequestMessage request)
        {
            // Resolve options when the policy is needed. This approach uses the options
            // configured at the time the HttpClient is created or the policy handler is first invoked.
            // For truly dynamic options per request (rarely needed for base settings like retries),
            // Polly Context would be required.
            var geminiOptions = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;
            var maxRetries = geminiOptions.MaxRetries > 0 ? geminiOptions.MaxRetries : 3; // Default 3 retries

            var logger = serviceProvider.GetService<ILogger<GeminiClient>>(); // Get logger for logging retries

            // Use Polly extensions for common transient error handling
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException, 5xx, 408 (Request Timeout)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Optionally retry on 429
                .WaitAndRetryAsync(
                    retryCount: maxRetries,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential back-off: 2s, 4s, 8s,...
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        // Log retries using the resolved logger
                        logger?.LogWarning(
                            "Request to {RequestUri} failed with {StatusCode}. Delaying for {Timespan}ms, then making retry {RetryAttempt} of {MaxRetries}...",
                            outcome.Result?.RequestMessage?.RequestUri ?? request.RequestUri, // Use request URI from outcome if available
                            outcome.Result?.StatusCode,
                            timespan.TotalMilliseconds,
                            retryAttempt,
                            maxRetries);
                    }
                );
        }
    }
}