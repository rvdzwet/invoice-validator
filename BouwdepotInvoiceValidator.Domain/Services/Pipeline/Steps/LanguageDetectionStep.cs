using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Models.AdvancedDocumentAnalysis;
using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Pipeline step for detecting the language of a document
    /// </summary>
    public class LanguageDetectionStep : IValidationPipelineStep
    {
        private readonly ILogger<LanguageDetectionStep> _logger;
        private readonly PromptService _promptService;
        
        /// <summary>
        /// Gets the name of the step
        /// </summary>
        public string StepName => "DetectLanguage";
        
        /// <summary>
        /// Gets the order of the step in the pipeline
        /// </summary>
        public int Order => 100; // First step
        
        /// <summary>
        /// Gets the prompt template name for this step
        /// </summary>
        public string PromptTemplatePath => "LanguageDetection";
        
        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        public Type ResponseType => typeof(AdvancedDocumentAnalysisOutput);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageDetectionStep"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        public LanguageDetectionStep(
            ILogger<LanguageDetectionStep> logger,
            PromptService promptService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        }
        
        /// <inheritdoc/>
        public bool ShouldExecute(ValidationContext context)
        {
            // Always execute this step unless we've already determined the document is invalid
            return context.OverallOutcome != ValidationOutcome.Invalid && 
                   context.OverallOutcome != ValidationOutcome.Error;
        }
        
        /// <inheritdoc/>
        public async Task<(string Prompt, List<Stream> ImageStreams, List<string> MimeTypes)> PreparePromptAsync(ValidationContext context, Stream documentStream)
        {
            _logger.LogInformation("Preparing language detection prompt: {ContextId}", context.Id);
            context.AddProcessingStep(StepName, "Detecting document language", ProcessingStepStatus.InProgress);
            
            var prompt = _promptService.GetPrompt(PromptTemplatePath).Render();
            
            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }
        
        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var analysisResult = (AdvancedDocumentAnalysisOutput)response;
            
            context.Language = analysisResult.Language;
            
            context.AddProcessingStep(StepName,
                $"Detected language: {context.Language.Name} (confidence: {context.Language.Confidence:P0}, explanation: {context.Language.Explanation})",
                ProcessingStepStatus.Success);
            
            context.AddAIModelUsage("Gemini", "1.0", "LanguageDetection", 100);
            
            await Task.CompletedTask; // To satisfy the async signature
        }
    }
}
