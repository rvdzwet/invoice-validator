using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Pipeline step for extracting the structure of a document
    /// </summary>
    public class DocumentStructureExtractionStep : IValidationPipelineStep
    {
        private readonly ILogger<DocumentStructureExtractionStep> _logger;
        private readonly PromptService _promptService;
        
        /// <summary>
        /// Gets the name of the step
        /// </summary>
        public string StepName => "ExtractDocumentStructure";
        
        /// <summary>
        /// Gets the order of the step in the pipeline
        /// </summary>
        public int Order => 300; // After document type verification (200)
        
        /// <summary>
        /// Gets the prompt template name for this step
        /// </summary>
        public string PromptTemplatePath => "InvoiceHeaderExtraction";
        
        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        public Type ResponseType => typeof(InvoiceHeaderResponse);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentStructureExtractionStep"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        public DocumentStructureExtractionStep(
            ILogger<DocumentStructureExtractionStep> logger,
            PromptService promptService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        }
        
        /// <inheritdoc/>
        public bool ShouldExecute(ValidationContext context)
        {
            // Only execute if the document is an invoice or receipt
            return (context.DocumentType == "invoice" || context.DocumentType == "receipt") &&
                   context.OverallOutcome != ValidationOutcome.Invalid &&
                   context.OverallOutcome != ValidationOutcome.Error;
        }
        
        /// <inheritdoc/>
        public async Task<(string Prompt, List<Stream> ImageStreams, List<string> MimeTypes)> PreparePromptAsync(ValidationContext context, Stream documentStream)
        {
            _logger.LogInformation("Preparing document structure extraction prompt: {ContextId}", context.Id);
            context.AddProcessingStep(StepName, "Extracting document structure", ProcessingStepStatus.InProgress);
            
            var prompt = _promptService.GetPrompt(PromptTemplatePath).Render();
            
            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }
        
        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var headerResponse = (InvoiceHeaderResponse)response;
            
            // Create a new invoice object if it doesn't exist
            if (context.ExtractedInvoice == null)
            {
                context.ExtractedInvoice = new Invoice
                {
                    FileName = context.InputDocument.FileName,
                    FileSizeBytes = context.InputDocument.FileSizeBytes
                };
            }
            
            // Update the invoice with the extracted header information
            context.ExtractedInvoice.InvoiceNumber = headerResponse.invoiceNumber;
            context.ExtractedInvoice.InvoiceDate = InvoiceValidationHelpers.ParseDate(headerResponse.invoiceDate);
            context.ExtractedInvoice.DueDate = InvoiceValidationHelpers.ParseDate(headerResponse.dueDate);
            context.ExtractedInvoice.TotalAmount = headerResponse.totalAmount;
            context.ExtractedInvoice.VatAmount = headerResponse.taxAmount;
            context.ExtractedInvoice.Currency = headerResponse.currency;
            
            context.AddProcessingStep(StepName,
                $"Extracted invoice header: Invoice #{headerResponse.invoiceNumber}, Date: {headerResponse.invoiceDate}, Total: {headerResponse.totalAmount} {headerResponse.currency}",
                ProcessingStepStatus.Success);
            
            context.AddAIModelUsage("Gemini", "1.0", "DocumentStructureExtraction", 500);
            
            await Task.CompletedTask; // To satisfy the async signature
        }
    }
}
