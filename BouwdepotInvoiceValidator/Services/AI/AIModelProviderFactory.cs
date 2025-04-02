using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Factory for creating appropriate AI model providers
    /// </summary>
    public class AIModelProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIModelProviderFactory> _logger;
        
        public AIModelProviderFactory(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<AIModelProviderFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }
        
        /// <summary>
        /// Creates an AI model provider based on configuration
        /// </summary>
        /// <returns>Appropriate IAIModelProvider implementation</returns>
        public IAIModelProvider CreateProvider()
        {
            // Get provider type from configuration
            string providerType = _configuration.GetValue<string>("AIService:ProviderType", "Gemini");
            
            _logger.LogInformation("Creating AI model provider of type: {ProviderType}", providerType);
            
            // Create appropriate provider based on configuration
            switch (providerType.ToLowerInvariant())
            {
                case "gemini":
                    return _serviceProvider.GetRequiredService<GeminiModelProvider>();
                    
                // Add more provider types as needed
                // case "openai":
                //     return _serviceProvider.GetRequiredService<OpenAIModelProvider>();
                
                default:
                    _logger.LogWarning("Unknown provider type: {ProviderType}, falling back to Gemini", providerType);
                    return _serviceProvider.GetRequiredService<GeminiModelProvider>();
            }
        }
        
        /// <summary>
        /// Registers AI model providers in the service collection
        /// </summary>
        public static void RegisterProviders(IServiceCollection services)
        {
            // Register the factory
            services.AddSingleton<AIModelProviderFactory>();
            
            // Register model providers
            services.AddSingleton<GeminiModelProvider>();
            
            // Add more provider registrations as needed
            // services.AddSingleton<OpenAIModelProvider>();
        }
    }
}
