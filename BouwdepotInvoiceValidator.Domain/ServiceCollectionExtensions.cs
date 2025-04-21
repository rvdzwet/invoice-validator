﻿﻿using BouwdepotInvoiceValidator.Domain.Services.Pipeline;
using BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps;
using BouwdepotInvoiceValidator.Domain.Services.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Extension methods for configuring services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds document validation services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDocumentValidation(this IServiceCollection services)
        {
            // Register pipeline steps
            services.AddScoped<ComprehensiveWithdrawalProofValidationStep>();
            
            // Register all steps as a collection for the pipeline
            services.AddScoped<IEnumerable<IValidationPipelineStep>>(sp => new List<IValidationPipelineStep>
            {
                sp.GetRequiredService<ComprehensiveWithdrawalProofValidationStep>(),
            });
            
            // Register the pipeline
            services.AddScoped<IValidationPipeline, ValidationPipeline>();

            services.AddScoped<IWithdrawalProofValidationService, WithdrawalProofValidationService>();
            services.AddSingleton<IJsonSchemaGenerator, JsonSchemaGenerator>();
            services.AddSingleton<DynamicPromptService>();

            return services;
        }
    }
}
