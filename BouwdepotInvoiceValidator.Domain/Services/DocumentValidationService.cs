using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services.Pipeline;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Service for validating documents (invoices, receipts) using a pipeline approach
    /// </summary>
    public class DocumentValidationService : IInvoiceValidationService
    {
        private readonly IValidationPipeline _pipeline;
        private readonly ILogger<DocumentValidationService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentValidationService"/> class
        /// </summary>
        /// <param name="pipeline">The validation pipeline</param>
        /// <param name="logger">The logger</param>
        public DocumentValidationService(
            IValidationPipeline pipeline,
            ILogger<DocumentValidationService> logger)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc/>
        public async Task<ValidationResult> ValidateInvoiceAsync(Stream fileStream, string fileName, string contentType)
        {
            _logger.LogInformation("Starting document validation for file: {FileName}", fileName);
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Create a new validation context
                var context = new ValidationContext();
                
                // Initialize input document info
                context.InputDocument = new InputDocumentInfo
                {
                    FileName = fileName,
                    ContentType = contentType,
                    FileSizeBytes = fileStream.Length,
                    UploadTimestamp = DateTimeOffset.UtcNow
                };
                
                // Execute the pipeline
                context = await _pipeline.ExecuteAsync(context, fileStream);
                
                _logger.LogInformation("Document validation completed for file: {FileName}, ContextId: {ContextId}",
                    fileName, context.Id);
                
                return InvoiceValidationHelpers.MapToValidationResult(context, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating document: {FileName}", fileName);
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }
    }
}
