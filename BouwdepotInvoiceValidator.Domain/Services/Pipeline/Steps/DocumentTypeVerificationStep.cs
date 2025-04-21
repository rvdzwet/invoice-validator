using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Pipeline step for verifying the document type (invoice, receipt, etc.)
    /// </summary>
    public class DocumentTypeVerificationStep : IValidationPipelineStep
    {
        private readonly ILogger<DocumentTypeVerificationStep> _logger;
        private readonly PromptService _promptService;
        
        /// <summary>
        /// Gets the name of the step
        /// </summary>
        public string StepName => "VerifyDocumentType";
        
        /// <summary>
        /// Gets the order of the step in the pipeline
        /// </summary>
        public int Order => 200; // After language detection (100)
        
        /// <summary>
        /// Gets the prompt template name for this step
        /// </summary>
        public string PromptTemplatePath => "DocumentTypeVerification";
        
        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        public Type ResponseType => typeof(DocumentTypeVerificationResponse);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeVerificationStep"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        public DocumentTypeVerificationStep(
            ILogger<DocumentTypeVerificationStep> logger,
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
            _logger.LogInformation("Preparing document type verification prompt: {ContextId}", context.Id);
            context.AddProcessingStep(StepName, "Verifying document type", ProcessingStepStatus.InProgress);
            
            // Get the document type verification prompt
            var prompt = _promptService.GetPrompt(PromptTemplatePath).Render();
            
            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }
        
        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var typedResponse = (DocumentTypeVerificationResponse)response;
            
            // Update context based on response
            if (typedResponse.isInvoice || typedResponse.isReceipt || typedResponse.isQuotation)
            {
                string documentType = typedResponse.isInvoice ? "invoice" : 
                                     (typedResponse.isReceipt ? "receipt" : "quotation");
                
                context.AddProcessingStep(StepName,
                    $"Document verified as {documentType} (confidence: {typedResponse.confidence}%)",
                    ProcessingStepStatus.Success);
                
                // Store the document type in the context for later steps
                context.DocumentType = documentType;
            }
            else
            {
                context.AddProcessingStep(StepName,
                    $"Document is not a valid withdrawal proof document. Detected as: {typedResponse.documentType}",
                    ProcessingStepStatus.Warning);
                
                context.AddIssue("InvalidDocumentType",
                    $"The document appears to be a {typedResponse.documentType}, not an invoice, receipt, or quotation. {typedResponse.explanation}",
                    IssueSeverity.Error);
                
                context.OverallOutcome = ValidationOutcome.Invalid;
                context.OverallOutcomeSummary = $"Document is not a valid withdrawal proof document. It appears to be a {typedResponse.documentType}.";
            }
            
            // Record AI model usage
            context.AddAIModelUsage("Gemini", "1.0", "DocumentTypeVerification", 500);
            
            await Task.CompletedTask; // To satisfy the async signature
        }
    }
}
