using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Domain.Models.Analysis;
using BouwdepotInvoiceValidator.Domain.Models.API;
using BouwdepotInvoiceValidator.Domain.Services.AI;
using BouwdepotInvoiceValidator.Domain.Services.Audit;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Main service for validating invoices using AI-powered analysis
    /// </summary>
    public class InvoiceValidationService : IInvoiceValidationService
    {
        private readonly ILogger<InvoiceValidationService> _logger;
        private readonly ILanguageDetector _languageDetector;
        private readonly IDocumentLayoutAnalyzer _documentLayoutAnalyzer;
        private readonly IInvoiceStructureExtractor _invoiceStructureExtractor;
        private readonly IFraudDetector _fraudDetector;
        private readonly IBouwdepotRulesValidator _rulesValidator;
        private readonly IAuditReportService _auditReportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceValidationService"/> class.
        /// </summary>
        public InvoiceValidationService(
            ILogger<InvoiceValidationService> logger,
            ILanguageDetector languageDetector,
            IDocumentLayoutAnalyzer documentLayoutAnalyzer,
            IInvoiceStructureExtractor invoiceStructureExtractor,
            IFraudDetector fraudDetector,
            IBouwdepotRulesValidator rulesValidator,
            IAuditReportService auditReportService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _languageDetector = languageDetector ?? throw new ArgumentNullException(nameof(languageDetector));
            _documentLayoutAnalyzer = documentLayoutAnalyzer ?? throw new ArgumentNullException(nameof(documentLayoutAnalyzer));
            _invoiceStructureExtractor = invoiceStructureExtractor ?? throw new ArgumentNullException(nameof(invoiceStructureExtractor));
            _fraudDetector = fraudDetector ?? throw new ArgumentNullException(nameof(fraudDetector));
            _rulesValidator = rulesValidator ?? throw new ArgumentNullException(nameof(rulesValidator));
            _auditReportService = auditReportService ?? throw new ArgumentNullException(nameof(auditReportService));
        }

        /// <inheritdoc/>
        public async Task<SimplifiedValidationResponse> ValidateInvoiceAsync(Stream fileStream, string fileName, string contentType)
        {
            _logger.LogInformation("Starting invoice validation for file: {FileName}", fileName);
            
            try
            {
                // Perform the full analysis
                var context = await AnalyzeInvoiceAsync(fileStream, fileName, contentType);
                
                // Generate a simplified response for the API
                var response = await _auditReportService.GenerateSimplifiedResponseAsync(context);
                
                _logger.LogInformation("Invoice validation completed for file: {FileName}, ValidationId: {ValidationId}", 
                    fileName, response.ValidationId);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice: {FileName}", fileName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<InvoiceAnalysisContext> AnalyzeInvoiceAsync(Stream fileStream, string fileName, string contentType)
        {
            _logger.LogInformation("Starting invoice analysis for file: {FileName}", fileName);
            
            try
            {
                // Create a new analysis context
                var context = new InvoiceAnalysisContext();
                
                // Initialize input document info
                context.InputDocument = new InputDocumentInfo
                {
                    FileName = fileName,
                    ContentType = contentType,
                    FileSizeBytes = fileStream.Length,
                    UploadTimestamp = DateTimeOffset.UtcNow
                };
                
                // Add initial processing step
                context.AddProcessingStep("InitializeAnalysis", 
                    "Starting invoice analysis process", 
                    ProcessingStepStatus.Success);
                
                // Step 1: Detect language
                await _languageDetector.DetectLanguageAsync(context);
                
                // Step 2: Analyze document layout
                await _documentLayoutAnalyzer.AnalyzeDocumentLayoutAsync(context);
                
                // Step 3: Extract invoice structure
                await _invoiceStructureExtractor.ExtractInvoiceStructureAsync(context);
                
                // Step 4: Detect fraud
                await _fraudDetector.DetectFraudAsync(context);
                
                // Step 5: Validate against Bouwdepot rules
                await _rulesValidator.ValidateInvoiceAsync(context);
                
                // Step 6: Generate and persist audit report
                var auditReport = await _auditReportService.GenerateAuditReportAsync(context);
                var reportId = await _auditReportService.PersistAuditReportAsync(auditReport);
                
                // Add final processing step
                context.AddProcessingStep("CompleteAnalysis", 
                    "Invoice analysis process completed", 
                    ProcessingStepStatus.Success);
                
                _logger.LogInformation("Invoice analysis completed for file: {FileName}, ContextId: {ContextId}", 
                    fileName, context.Id);
                
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing invoice: {FileName}", fileName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SimplifiedValidationResponse> GetValidationResultAsync(string validationId)
        {
            _logger.LogInformation("Retrieving validation result for ID: {ValidationId}", validationId);
            
            try
            {
                // This is a simplified implementation. In a real system, you would retrieve
                // the validation result from a database or other persistent storage.
                
                // For now, we'll just return null to indicate that the validation result was not found
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving validation result: {ValidationId}", validationId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<InvoiceAnalysisContext> GetAnalysisContextAsync(string analysisId)
        {
            _logger.LogInformation("Retrieving analysis context for ID: {AnalysisId}", analysisId);
            
            try
            {
                // This is a simplified implementation. In a real system, you would retrieve
                // the analysis context from a database or other persistent storage.
                
                // For now, we'll just return null to indicate that the analysis context was not found
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis context: {AnalysisId}", analysisId);
                throw;
            }
        }
    }
}
