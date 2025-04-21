using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Pipeline step for detecting fraud in a document
    /// </summary>
    public class FraudDetectionStep : IValidationPipelineStep
    {
        private readonly ILogger<FraudDetectionStep> _logger;
        private readonly PromptService _promptService;
        
        /// <summary>
        /// Gets the name of the step
        /// </summary>
        public string StepName => "DetectFraud";
        
        /// <summary>
        /// Gets the order of the step in the pipeline
        /// </summary>
        public int Order => 400; // After document structure extraction (300)
        
        /// <summary>
        /// Gets the prompt template name for this step
        /// </summary>
        public string PromptTemplatePath => "FraudDetection";
        
        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        public Type ResponseType => typeof(FraudDetectionResponse);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FraudDetectionStep"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        public FraudDetectionStep(
            ILogger<FraudDetectionStep> logger,
            PromptService promptService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        }
        
        /// <inheritdoc/>
        public bool ShouldExecute(ValidationContext context)
        {
            // Only execute if we have extracted invoice data
            return context.ExtractedInvoice != null &&
                   context.OverallOutcome != ValidationOutcome.Invalid &&
                   context.OverallOutcome != ValidationOutcome.Error;
        }
        
        /// <inheritdoc/>
        public async Task<(string Prompt, List<Stream> ImageStreams, List<string> MimeTypes)> PreparePromptAsync(ValidationContext context, Stream documentStream)
        {
            _logger.LogInformation("Preparing fraud detection prompt: {ContextId}", context.Id);
            context.AddProcessingStep(StepName, "Detecting potential fraud", ProcessingStepStatus.InProgress);
            
            var prompt = _promptService.GetPrompt(PromptTemplatePath).Render();
            
            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }
        
        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var fraudResponse = (FraudDetectionResponse)response;
            
            // Create a fraud analysis result
            context.FraudAnalysis = new FraudAnalysisResult
            {
                RiskLevel = fraudResponse.possibleFraud ? "HIGH" : "LOW",
                RiskScore = (int)(fraudResponse.confidence * 100),
                Summary = fraudResponse.visualEvidence,
                Indicators = fraudResponse.visualIndicators?.Select(i => new FraudIndicator
                {
                    Category = "Visual",
                    Description = i,
                    Confidence = fraudResponse.confidence
                }).ToList() ?? new List<FraudIndicator>()
            };
            
            if (fraudResponse.possibleFraud)
            {
                context.AddProcessingStep(StepName,
                    $"Potential fraud detected (confidence: {fraudResponse.confidence:P0}): {fraudResponse.visualEvidence}",
                    ProcessingStepStatus.Warning);
                
                context.AddIssue("PotentialFraud",
                    $"Potential fraud detected: {fraudResponse.visualEvidence}",
                    IssueSeverity.Warning);
                
                context.OverallOutcome = ValidationOutcome.NeedsReview;
                context.OverallOutcomeSummary = "Document needs review due to potential fraud indicators.";
            }
            else
            {
                context.AddProcessingStep(StepName,
                    "No fraud indicators detected",
                    ProcessingStepStatus.Success);
            }
            
            context.AddAIModelUsage("Gemini", "1.0", "FraudDetection", 500);
            
            await Task.CompletedTask; // To satisfy the async signature
        }
    }
}
