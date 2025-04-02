using System;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models.Enhanced; // Add this using statement

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Unified invoice validation service that uses the AI model provider abstraction
    /// This service consolidates all validation steps into a single comprehensive process
    /// </summary>
    public class UnifiedInvoiceValidationService : IInvoiceValidationService
    {
        private readonly ILogger<UnifiedInvoiceValidationService> _logger;
        private readonly AIModelProviderFactory _modelProviderFactory;
        private readonly IPdfExtractionService _pdfExtractionService;
        private readonly IConfiguration _configuration;
        
        public UnifiedInvoiceValidationService(
            ILogger<UnifiedInvoiceValidationService> logger,
            AIModelProviderFactory modelProviderFactory,
            IPdfExtractionService pdfExtractionService,
            IConfiguration configuration)
        {
            _logger = logger;
            _modelProviderFactory = modelProviderFactory;
            _pdfExtractionService = pdfExtractionService;
            _configuration = configuration;
        }
        
        /// <summary>
        /// Gets the AI model provider
        /// </summary>
        private IAIModelProvider ModelProvider => _modelProviderFactory.CreateProvider();
        
        /// <inheritdoc />
        public async Task<ValidationResult> ValidateInvoiceAsync(System.IO.Stream fileStream, string fileName)
        {
            _logger.LogInformation("Starting unified validation of invoice: {FileName}", fileName);
            
            try
            {
                // Step 1: Check for tampering
                _logger.LogInformation("Checking for PDF tampering signs");
                var possibleTampering = await _pdfExtractionService.DetectTamperingAsync(fileStream);
                
                if (possibleTampering)
                {
                    _logger.LogWarning("Tampering detected in PDF file: {FileName}", fileName);
                    
                    // Create a validation result with tampering error
                    var tamperingResult = new ValidationResult
                    {
                        IsValid = false,
                        PossibleTampering = true
                    };
                    
                    tamperingResult.AddIssue(ValidationSeverity.Error, 
                        "PDF tampering detected. This file may have been modified.");
                    
                    return tamperingResult;
                }
                
                // Step 2: Extract invoice data from PDF
                _logger.LogInformation("Extracting data from invoice PDF");
                var invoice = await _pdfExtractionService.ExtractFromPdfAsync(fileStream, fileName);
                
                // Step 3: Extract page images for multi-modal validation
                bool useMultiModalValidation = _configuration.GetValue<bool>("Validation:UseMultiModalAnalysis", true);
                if (useMultiModalValidation)
                {
                    _logger.LogInformation("Extracting page images for multi-modal validation");
                    invoice.PageImages = await _pdfExtractionService.ExtractPageImagesAsync(fileStream);
                }
                
                // Step 4: Prepare validation options
                var options = new ValidationOptions
                {
                    LanguageCode = _configuration.GetValue<string>("Validation:LanguageCode", "nl-NL"),
                    ConfidenceThreshold = _configuration.GetValue<int>("Validation:ConfidenceThreshold", 70),
                    IncludeVisualAnalysis = useMultiModalValidation,
                    DetectFraud = _configuration.GetValue<bool>("Validation:DetectFraud", true),
                    IncludeAuditJustification = _configuration.GetValue<bool>("Validation:IncludeAuditJustification", true)
                };
                
                // Step 5: Perform validation using the model provider
                _logger.LogInformation("Performing comprehensive invoice validation with AI model");
                var aiResponse = await ModelProvider.ValidateInvoiceAsync(invoice, options);
                
                // Step 6: Map AI response to our validation result model
                var result = MapToValidationResult(aiResponse);
                result.ValidationId = Guid.NewGuid().ToString();
                result.ValidatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Invoice validation completed: {FileName}, IsValid={IsValid}, Score={Score}", 
                    fileName, result.IsValid, result.ConfidenceScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice {FileName}", fileName);
                
                var errorResult = new ValidationResult { IsValid = false };
                errorResult.AddIssue(ValidationSeverity.Error, 
                    $"Error processing invoice: {ex.Message}");
                
                return errorResult;
            }
        }
        
        /// <inheritdoc />
        public Task<ValidationResult> GetValidationResultAsync(string validationId)
        {
            // In a real implementation, this would retrieve the result from a database
            // For now, we'll return a mocked result for demo purposes
            
            _logger.LogInformation("Retrieving validation result for ID: {ValidationId}", validationId);
            
            if (string.IsNullOrWhiteSpace(validationId))
            {
                _logger.LogWarning("Attempted to retrieve validation result with null or empty ID");
                return Task.FromResult<ValidationResult>(null);
            }
            
            // Create a mock result for demonstration
            var mockResult = new ValidationResult
            {
                ValidationId = validationId,
                IsValid = true,
                IsHomeImprovement = true,
                IsBouwdepotCompliant = true,
                ConfidenceScore = 85,
                MeetsApprovalThreshold = true,
                ValidatedAt = DateTime.UtcNow.AddDays(-1),
                ExtractedInvoice = new Invoice
                {
                    InvoiceNumber = $"INV-{validationId}",
                    VendorName = "Example Construction Inc.",
                    TotalAmount = 4250.00m,
                    InvoiceDate = DateTime.Now.AddDays(-7),
                }
            };
            
            mockResult.AddIssue(ValidationSeverity.Info, 
                "This is a mock validation result. In a real implementation, this would be retrieved from a database.");
            
            return Task.FromResult(mockResult);
        }

        /// <inheritdoc />
        public Task<int> ProcessAdditionalPagesAsync(string validationId, int startPage = 2)
        {
            // This would process additional pages, but in our unified approach
            // we process all pages at once during the initial validation
            _logger.LogInformation("ProcessAdditionalPagesAsync called but not required in unified approach");
            return Task.FromResult(0);
        }
        
        /// <summary>
        /// Maps from the AI model response to our ValidationResult model
        /// </summary>
        private ValidationResult MapToValidationResult(InvoiceValidationResponse aiResponse)
        {
            var result = new ValidationResult
            {
                ExtractedInvoice = aiResponse.ExtractedInvoice,
                IsValid = aiResponse.IsValidInvoice,
                IsHomeImprovement = aiResponse.IsHomeImprovement,
                ConfidenceScore = aiResponse.ConfidenceScore,
                RawGeminiResponse = aiResponse.RawResponse,
                MeetsApprovalThreshold = aiResponse.ConfidenceScore >= 70,
                AuditJustification = aiResponse.AuditJustification
            };
            
            // If bouwdepot validation is available, apply it
            if (aiResponse.BouwdepotValidation != null)
            {
                result.BouwdepotValidation = aiResponse.BouwdepotValidation;
                
                // Determine Bouwdepot compliance based on both rules
                result.IsBouwdepotCompliant = 
                    aiResponse.BouwdepotValidation.QualityImprovementRule && 
                    aiResponse.BouwdepotValidation.PermanentAttachmentRule;
            }
            
            // Add purchase analysis if available
            if (aiResponse.LineItemAnalysis != null && aiResponse.LineItemAnalysis.Count > 0)
            {
                result.PurchaseAnalysis = new PurchaseAnalysis
                {
                    Summary = aiResponse.Summary,
                    PrimaryPurpose = aiResponse.PrimaryPurpose,
                    Categories = aiResponse.Categories,
                    HomeImprovementPercentage = CalculateHomeImprovementPercentage(aiResponse.LineItemAnalysis)
                };
                
                // Add line item details
                foreach (var item in aiResponse.LineItemAnalysis)
                {
                    result.PurchaseAnalysis.LineItemDetails.Add(new BouwdepotInvoiceValidator.Models.LineItemAnalysisDetail
                    {
                        Description = item.Description,
                        InterpretedAs = item.InterpretedAs,
                        Category = item.Category,
                        IsHomeImprovement = item.IsEligible,
                        Confidence = item.Confidence / 100.0, // Convert from 0-100 to 0-1 scale
                        Notes = item.Notes
                    });
                }
            }
            
            // Add validation issues
            foreach (var note in aiResponse.ValidationNotes)
            {
                result.AddIssue(note.Severity, note.Message);
            }
            
            // Add fraud detection if available
            if (aiResponse.FraudIndicators != null && aiResponse.FraudIndicators.Count > 0)
            {
                result.FraudDetection = new FraudDetection
                {
                    FraudRiskScore = CalculateFraudRiskScore(aiResponse.FraudIndicators),
                    RiskLevel = FraudRiskLevel.Low // Default, will be updated below
                };
                
                // Add detected indicators
                foreach (var indicator in aiResponse.FraudIndicators)
                {
                    // Use the FraudIndicator from the Enhanced namespace
                    result.FraudDetection.DetectedIndicators.Add(new BouwdepotInvoiceValidator.Models.Enhanced.FraudIndicator 
                    {
                        IndicatorName = indicator.IndicatorName, // Property exists in AI.FraudIndicator and Enhanced.FraudIndicator
                        Description = indicator.Description, // Property exists in AI.FraudIndicator and Enhanced.FraudIndicator
                        Evidence = indicator.Evidence,
                        Severity = indicator.Severity, // Property is double in both AI.FraudIndicator and Enhanced.FraudIndicator
                        Category = (BouwdepotInvoiceValidator.Models.Enhanced.FraudIndicatorCategory)indicator.Category // Explicit cast needed
                    });
                }
                
                // Update risk level based on score
                result.FraudDetection.RiskLevel = result.FraudDetection.FraudRiskScore switch
                {
                    < 25 => FraudRiskLevel.Low,
                    < 50 => FraudRiskLevel.Medium,
                    < 75 => FraudRiskLevel.High,
                    _ => FraudRiskLevel.Critical
                };
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculates the percentage of items that are home improvement related
        /// </summary>
        private double CalculateHomeImprovementPercentage(List<LineItemValidation> lineItems)
        {
            if (lineItems == null || lineItems.Count == 0)
                return 0;
                
            int homeImprovementCount = 0;
            foreach (var item in lineItems)
            {
                if (item.IsEligible)
                    homeImprovementCount++;
            }
            
            return (double)homeImprovementCount / lineItems.Count;
        }
        
        /// <summary>
        /// Calculates a fraud risk score based on detected indicators
        /// </summary>
        private int CalculateFraudRiskScore(List<BouwdepotInvoiceValidator.Services.AI.FraudIndicator> indicators)
        {
            if (indicators == null || indicators.Count == 0)
                return 0;
                
            double totalSeverity = 0;
            foreach (var indicator in indicators)
            {
                totalSeverity += indicator.Severity;
            }
            
            // Scale to 0-100
            double maxPossibleSeverity = indicators.Count * 1.0; // Max severity is 1.0 per indicator
            double scaledScore = (totalSeverity / maxPossibleSeverity) * 100;
            
            return (int)Math.Min(100, scaledScore);
        }
    }
}
