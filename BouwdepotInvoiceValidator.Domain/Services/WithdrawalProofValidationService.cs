using Microsoft.Extensions.Logging;

using System.Diagnostics;

using BouwdepotInvoiceValidator.Domain.Services.Pipeline;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Main service for validating construction fund withdrawal proof documents using AI-powered analysis
    /// </summary>
    internal class WithdrawalProofValidationService : IWithdrawalProofValidationService
    {
        private readonly ILogger<WithdrawalProofValidationService> _logger;
        private readonly IValidationPipeline _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="WithdrawalProofValidationService"/> class.
        /// </summary>
        public WithdrawalProofValidationService(
            ILogger<WithdrawalProofValidationService> logger,
            IValidationPipeline validationPipeline)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validationPipeline = validationPipeline ?? throw new ArgumentNullException(nameof(validationPipeline));
        }

        /// <inheritdoc/>
        public async Task<ValidationContext> ValidateWithdrawalProofAsync(Stream fileStream, string fileName, string contentType)
        {
            _logger.LogInformation("Starting withdrawal proof validation for file: {FileName}", fileName);
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
                context = await _validationPipeline.ExecuteAsync(context, fileStream);

                _logger.LogInformation("Withdrawal proof validation completed for file: {FileName}, ContextId: {ContextId}",
                    fileName, context.Id);

                context.ElapsedTime = stopwatch.Elapsed;

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating withdrawal proof document: {FileName}", fileName);
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }
    }
}
