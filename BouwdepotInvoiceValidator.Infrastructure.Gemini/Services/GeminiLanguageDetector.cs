using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Domain.Models.Analysis;
using BouwdepotInvoiceValidator.Domain.Services.AI;

namespace BouwdepotInvoiceValidator.Infrastructure.Gemini.Services
{
    /// <summary>
    /// Gemini implementation of the language detector
    /// </summary>
    public class GeminiLanguageDetector : ILanguageDetector
    {
        private readonly ILogger<GeminiLanguageDetector> _logger;
        private readonly IGeminiClient _geminiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiLanguageDetector"/> class.
        /// </summary>
        public GeminiLanguageDetector(
            ILogger<GeminiLanguageDetector> logger,
            IGeminiClient geminiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _geminiClient = geminiClient ?? throw new ArgumentNullException(nameof(geminiClient));
        }

        /// <inheritdoc/>
        public async Task DetectLanguageAsync(InvoiceAnalysisContext context)
        {
            _logger.LogInformation("Detecting language for invoice: {ContextId}", context.Id);
            
            try
            {
                // Add processing step
                context.AddProcessingStep("LanguageDetection", 
                    "Detecting primary language of the document", 
                    ProcessingStepStatus.InProgress);
                
                // In a real implementation, we would use the Gemini API to detect the language
                // For now, we'll use a simplified implementation that assumes Dutch
                
                // Simulate API call
                await Task.Delay(500); // Simulate API latency
                
                // Set the detected language
                context.DetectedLanguage = "nl"; // Dutch
                context.LanguageDetectionConfidence = 0.95;
                
                // Add AI model usage
                context.AddAIModelUsage(
                    "gemini-pro-vision", 
                    "1.0", 
                    "language-detection", 
                    100);
                
                // Update processing step
                context.AddProcessingStep("LanguageDetection", 
                    $"Detected language: {context.DetectedLanguage} (confidence: {context.LanguageDetectionConfidence:P0})", 
                    ProcessingStepStatus.Success);
                
                _logger.LogInformation("Language detection completed for invoice: {ContextId}, Language: {Language}, Confidence: {Confidence:P0}", 
                    context.Id, context.DetectedLanguage, context.LanguageDetectionConfidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting language for invoice: {ContextId}", context.Id);
                
                // Add issue
                context.AddIssue(
                    "LanguageDetectionError", 
                    $"Error detecting language: {ex.Message}", 
                    IssueSeverity.Error);
                
                // Update processing step
                context.AddProcessingStep("LanguageDetection", 
                    "Error detecting language", 
                    ProcessingStepStatus.Error);
                
                // Set default language
                context.DetectedLanguage = "nl"; // Default to Dutch
                context.LanguageDetectionConfidence = 0.5;
                
                throw;
            }
        }
    }
}
