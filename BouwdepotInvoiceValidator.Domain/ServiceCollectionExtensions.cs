﻿﻿using BouwdepotInvoiceValidator.Domain.Services.Pipeline;
using BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps;
using BouwdepotInvoiceValidator.Domain.Services.Schema;
using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Configuration;
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
            
            // Register the validation services
            services.AddScoped<IInvoiceValidationService, DocumentValidationService>();
            services.AddScoped<IWithdrawalProofValidationService, WithdrawalProofValidationService>();
            
            // Register supporting services
            services.AddSingleton<PromptService>();
            services.AddSingleton<IJsonSchemaGenerator, JsonSchemaGenerator>();
            services.AddSingleton<DynamicPromptService>();

            return services;
        }
    }
}
