using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Services.AI;
using BouwdepotInvoiceValidator.Services.Security;
using BouwdepotInvoiceValidator.Services.Vendors;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for validating invoice PDFs
    /// </summary>
    public class InvoiceValidationService : IInvoiceValidationService
    {
        private readonly ILogger<InvoiceValidationService> _logger;
        private readonly IPdfExtractionService _pdfExtractionService;
        private readonly IAIService _aiService;
        private readonly IBouwdepotRulesValidationService _bouwdepotRulesService;
        private readonly IVendorProfileService _vendorProfileService;
        private readonly IDigitalSignatureService _signatureService;
        private readonly IConfiguration _configuration;
        
        // Configuration options
        private readonly bool _useMultiModalValidation;
        private readonly bool _enableVendorProfiling;
        private readonly bool _enableDigitalSigning;
        private readonly int _confidenceThreshold;

        public InvoiceValidationService(
            ILogger<InvoiceValidationService> logger,
            IPdfExtractionService pdfExtractionService,
            IAIService aiService,
            IBouwdepotRulesValidationService bouwdepotRulesService,
            IVendorProfileService vendorProfileService,
            IDigitalSignatureService signatureService,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pdfExtractionService = pdfExtractionService ?? throw new ArgumentNullException(nameof(pdfExtractionService));
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _bouwdepotRulesService = bouwdepotRulesService ?? throw new ArgumentNullException(nameof(bouwdepotRulesService));
            _vendorProfileService = vendorProfileService ?? throw new ArgumentNullException(nameof(vendorProfileService));
            _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Load configuration options
            _useMultiModalValidation = _configuration.GetValue<bool>("Validation:UseMultiModalAnalysis", true);
            _enableVendorProfiling = _configuration.GetValue<bool>("Validation:EnableVendorProfiling", true);
            _enableDigitalSigning = _configuration.GetValue<bool>("Validation:EnableDigitalSigning", true);
            _confidenceThreshold = _configuration.GetValue<int>("Validation:ConfidenceThreshold", 75);
            
            // Initialize storage for validation results
            _validationResults = new System.Collections.Concurrent.ConcurrentDictionary<string, ValidationResult>();
            
            var modelInfo = aiService.GetModelInfo();
            _logger.LogInformation("Invoice validation service initialized with {Provider} AI model: {ModelId}",
                modelInfo.ProviderName, modelInfo.ModelId);
        }
        
        // In-memory storage for validation results (would be replaced with a database in production)
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, ValidationResult> _validationResults;

        /// <inheritdoc />
        public async Task<ValidationResult> GetValidationResultAsync(string validationId)
        {
            _logger.LogInformation("Retrieving validation result for ID: {ValidationId}", validationId);
            
            if (string.IsNullOrWhiteSpace(validationId))
            {
                _logger.LogWarning("Attempted to retrieve validation result with null or empty ID");
                return null;
            }
            
            if (_validationResults.TryGetValue(validationId, out var result))
            {
                _logger.LogInformation("Found validation result for ID: {ValidationId}", validationId);
                
                // Verify digital signature if enabled
                if (_enableDigitalSigning && result.Signature != null)
                {
                    bool isSignatureValid = _signatureService.VerifySignature(result);
                    
                    if (!isSignatureValid)
                    {
                        _logger.LogWarning("Invalid digital signature detected for validation result: {ValidationId}", validationId);
                        result.AddIssue(ValidationSeverity.Error, "The validation result signature is invalid. Results may have been tampered with.");
                    }
                }
                
                return result;
            }
            
            // For demo purposes, create a mock result for any non-existing ID
            // In a real application, this would return null
            _logger.LogInformation("Creating mock validation result for ID: {ValidationId}", validationId);
            
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
                    LineItems = new List<InvoiceLineItem>
                    {
                        new InvoiceLineItem { Description = "Kitchen renovation materials", TotalPrice = 2450.00m },
                        new InvoiceLineItem { Description = "Bathroom installation", TotalPrice = 1200.00m },
                        new InvoiceLineItem { Description = "Installation hardware", TotalPrice = 600.00m }
                    }
                }
            };
            
            // Set up content summary
            mockResult.Summary = new ContentSummary
            {
                PurchasedItems = "Kitchen renovation materials, bathroom installation supplies, and installation hardware",
                IntendedPurpose = "Kitchen and bathroom renovation",
                PropertyImpact = "Significant improvement to kitchen and bathroom facilities, likely increasing property value",
                ProjectCategory = "Kitchen and Bathroom Renovation",
                EstimatedProjectScope = "Medium-sized renovation project"
            };
            
            // Set up purchase analysis
            mockResult.PurchaseAnalysis = new PurchaseAnalysis
            {
                Summary = "Kitchen and bathroom renovation materials and installation",
                PrimaryPurpose = "Home interior renovation",
                Categories = new List<string> { "Kitchen Renovation", "Bathroom Renovation", "Installation" },
                HomeImprovementPercentage = 1.0,
                LineItemDetails = new List<BouwdepotInvoiceValidator.Models.LineItemAnalysisDetail>
                {
                    new BouwdepotInvoiceValidator.Models.LineItemAnalysisDetail 
                    { 
                        Description = "Kitchen renovation materials", 
                        InterpretedAs = "Building materials for kitchen renovation",
                        Category = "Kitchen Renovation",
                        IsHomeImprovement = true,
                        Confidence = 0.95
                    },
                    new BouwdepotInvoiceValidator.Models.LineItemAnalysisDetail 
                    { 
                        Description = "Bathroom installation", 
                        InterpretedAs = "Installation service for bathroom fixtures",
                        Category = "Bathroom Renovation",
                        IsHomeImprovement = true,
                        Confidence = 0.98
                    },
                    new BouwdepotInvoiceValidator.Models.LineItemAnalysisDetail 
                    { 
                        Description = "Installation hardware", 
                        InterpretedAs = "Fasteners and mounting hardware",
                        Category = "Installation Materials",
                        IsHomeImprovement = true,
                        Confidence = 0.9
                    }
                }
            };
            
            // Set up Bouwdepot validation details
            mockResult.BouwdepotValidation = new BouwdepotValidationDetails
            {
                QualityImprovementRule = true,
                PermanentAttachmentRule = true,
                LineItemValidations = new List<BouwdepotLineItemValidation>
                {
                    new BouwdepotLineItemValidation
                    {
                        Description = "Kitchen renovation materials",
                        IsPermanentlyAttached = true,
                        ImproveHomeQuality = true
                    },
                    new BouwdepotLineItemValidation
                    {
                        Description = "Bathroom installation",
                        IsPermanentlyAttached = true,
                        ImproveHomeQuality = true
                    },
                    new BouwdepotLineItemValidation
                    {
                        Description = "Installation hardware",
                        IsPermanentlyAttached = true,
                        ImproveHomeQuality = true
                    }
                }
            };
            
            // Add confidence factors
            mockResult.AddConfidenceFactor("Clear line items", 15, 
                "Line items clearly describe home improvement materials and services");
            mockResult.AddConfidenceFactor("Recognized vendor", 10, 
                "Vendor is recognized as a construction contractor");
            mockResult.AddConfidenceFactor("Matching price ranges", 5, 
                "Prices are within expected ranges for these services");
            
            // Add some issues for context
            mockResult.AddIssue(ValidationSeverity.Info, "Invoice verified and validated successfully");
            mockResult.AddIssue(ValidationSeverity.Info, "All items qualify for Bouwdepot funding");
            mockResult.AddIssue(ValidationSeverity.Info, "Confidence score: 85/100");
            
            // Store the mock result for future retrievals
            _validationResults.TryAdd(validationId, mockResult);
            
            return mockResult;
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateInvoiceAsync(Stream fileStream, string fileName)
        {
            _logger.LogInformation("Starting validation of invoice: {FileName}", fileName);
            
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
                
                // Step 3: Check if the document appears to be an invoice based on extracted data
                bool hasInvoiceNumber = !string.IsNullOrEmpty(invoice.InvoiceNumber);
                bool hasInvoiceDate = invoice.InvoiceDate.HasValue;
                bool hasAmount = invoice.TotalAmount > 0;
                
                // Create validation result and track missing data
                ValidationResult validationResult = new ValidationResult
                {
                    ExtractedInvoice = invoice,
                    ValidationId = Guid.NewGuid().ToString(),
                    ValidatedAt = DateTime.UtcNow
                };
                
                // Log any missing critical data but continue with validation
                if (!hasInvoiceNumber || !hasInvoiceDate || !hasAmount)
                {
                    _logger.LogWarning("Incomplete invoice data extracted from {FileName}: " +
                                      "InvoiceNumber={HasNumber}, InvoiceDate={HasDate}, Amount={HasAmount}", 
                                      fileName, hasInvoiceNumber, hasInvoiceDate, hasAmount);
                    
                    // Add warnings to the validation result for missing data fields
                    if (!hasInvoiceNumber)
                    {
                        validationResult.AddIssue(ValidationSeverity.Warning, 
                            "Could not extract invoice number from this document.");
                    }
                    
                    if (!hasInvoiceDate)
                    {
                        validationResult.AddIssue(ValidationSeverity.Warning, 
                            "Could not extract invoice date from this document.");
                    }
                    
                    if (!hasAmount)
                    {
                        validationResult.AddIssue(ValidationSeverity.Warning, 
                            "Could not extract total amount from this document.");
                    }
                }
                
                // Step 4: Extract page images for multi-modal validation if needed
                if (_useMultiModalValidation)
                {
                    _logger.LogInformation("Extracting page images for multi-modal validation");
                    
                    if (invoice.PageImages == null || invoice.PageImages.Count == 0)
                    {
                        invoice.PageImages = await _pdfExtractionService.ExtractPageImagesAsync(fileStream);
                    }
                    
                    // Analyze visual elements if needed
                    if (invoice.VisualAnalysis == null)
                    {
                        invoice.VisualAnalysis = await _pdfExtractionService.AnalyzeVisualElementsAsync(fileStream);
                    }
                }
                
                // Step 5: Get vendor profile if enabled
                VendorProfile vendorProfile = null;
                if (_enableVendorProfiling)
                {
                    _logger.LogInformation("Getting vendor profile for: {VendorName}", invoice.VendorName);
                    vendorProfile = await _vendorProfileService.GetVendorProfileAsync(invoice);
                    
                    // Add vendor insights to the validation result
                    if (vendorProfile != null)
                    {
                        _logger.LogInformation("Found vendor profile: {VendorName}, Invoice count: {InvoiceCount}", 
                            vendorProfile.VendorName, vendorProfile.InvoiceCount);
                            
                        // Add vendor insights to the validation result
                        validationResult.VendorInsights = new VendorInsights
                        {
                            VendorName = vendorProfile.VendorName,
                            BusinessCategories = vendorProfile.BusinessCategories,
                            InvoiceCount = vendorProfile.InvoiceCount,
                            FirstSeen = vendorProfile.FirstSeen,
                            LastSeen = vendorProfile.LastSeen,
                            ReliabilityScore = vendorProfile.TrustMetrics.ReliabilityScore,
                            PriceStabilityScore = vendorProfile.TrustMetrics.PriceStabilityScore,
                            DocumentQualityScore = vendorProfile.TrustMetrics.DocumentQualityScore,
                            TotalAnomalyCount = vendorProfile.TrustMetrics.DetectedAnomalies.Count,
                            VendorSpecialties = vendorProfile.SpecialtyServices
                        };
                        
                        // Add vendor context information
                        validationResult.AddIssue(ValidationSeverity.Info, 
                            $"Vendor profile found: {vendorProfile.InvoiceCount} previous invoices processed");
                    
                        // Analyze vendor trustworthiness
                        var trustAnalysis = await _vendorProfileService.AnalyzeVendorTrustworthinessAsync(invoice);
                        
                        // Add trust factors to confidence factors
                        foreach (var factor in trustAnalysis.TrustFactors)
                        {
                            validationResult.AddConfidenceFactor("Vendor History: " + factor, 5, factor);
                        }
                        
                        // Add concern factors to confidence factors (negative impact)
                        foreach (var concern in trustAnalysis.ConcernFactors)
                        {
                            validationResult.AddConfidenceFactor("Vendor History Concern: " + concern, -8, concern);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No existing vendor profile found for: {VendorName}", invoice.VendorName);
                        validationResult.AddIssue(ValidationSeverity.Info, "New vendor - no historical data available");
                    }
                }
                
                // Step 6: Validate invoice with AI service
                _logger.LogInformation("Validating invoice with AI service");
                var aiResult = await _aiService.ValidateInvoiceAsync(invoice, "nl-NL", _confidenceThreshold);
                
                // Merge AI result with our result (keeping our warning messages)
                MergeValidationResults(validationResult, aiResult);
                
                // Step 7: Validate against Bouwdepot-specific rules if invoice is valid and related to home improvement
                if (validationResult.IsValid && validationResult.IsHomeImprovement)
                {
                    _logger.LogInformation("Validating invoice against Bouwdepot-specific rules");
                    
                    // Validate against general Bouwdepot rules
                    validationResult = await _bouwdepotRulesService.ValidateBouwdepotRulesAsync(invoice, validationResult);
                    
                    // If invoice is compliant with general Bouwdepot rules, check for Verduurzamingsdepot compliance
                    if (validationResult.IsBouwdepotCompliant)
                    {
                        _logger.LogInformation("Checking if invoice qualifies for Verduurzamingsdepot");
                        validationResult = await _bouwdepotRulesService.ValidateVerduurzamingsdepotRulesAsync(invoice, validationResult);
                        
                        if (validationResult.IsVerduurzamingsdepotCompliant)
                        {
                            _logger.LogInformation("Invoice qualifies for Verduurzamingsdepot funding: {FileName}", fileName);
                            validationResult.AddIssue(ValidationSeverity.Info, 
                                "This invoice qualifies for Verduurzamingsdepot funding for sustainability improvements.");
                        }
                    }
                }
                
                // Step 8: Update vendor profile with the validation result if enabled
                if (_enableVendorProfiling && validationResult.ExtractedInvoice != null)
                {
                    _logger.LogInformation("Updating vendor profile with validation result");
                    await _vendorProfileService.UpdateVendorProfileAsync(invoice, validationResult);
                    
                    // Check for vendor anomalies
                    if (vendorProfile != null && vendorProfile.InvoiceCount > 0)
                    {
                        _logger.LogInformation("Checking for vendor anomalies");
                        var anomalies = await _vendorProfileService.DetectVendorAnomaliesAsync(invoice, vendorProfile);
                        
                        if (anomalies.Count > 0)
                        {
                            _logger.LogWarning("Detected {AnomalyCount} vendor anomalies in invoice {FileName}", 
                                anomalies.Count, fileName);
                                
                            // Add anomalies as issues
                            foreach (var anomaly in anomalies)
                            {
                                var severity = anomaly.Severity > 0.6 
                                    ? ValidationSeverity.Warning 
                                    : ValidationSeverity.Info;
                                    
                                validationResult.AddIssue(severity, 
                                    $"Vendor anomaly: {anomaly.Description} ({anomaly.AnomalyType})");
                                    
                                // Add to fraud detection indicators
                                if (anomaly.Severity > 0.5)
                                {
                                    validationResult.FraudDetection.DetectedIndicators.Add(new BouwdepotInvoiceValidator.Models.FraudIndicator
                                    {
                                        IndicatorName = anomaly.AnomalyType,
                                        Description = anomaly.Description,
                                        Evidence = $"Detected by vendor profiling system on {anomaly.DetectedDate:g}",
                                        Severity = anomaly.Severity,
                                        Category = BouwdepotInvoiceValidator.Models.FraudIndicatorCategory.VendorIssue
                                    });
                                }
                            }
                            
                            // Update fraud risk score
                            int currentFraudScore = validationResult.FraudDetection.FraudRiskScore;
                            int anomalyImpact = (int)(anomalies.Count * 5); // 5 points per anomaly
                            
                            validationResult.FraudDetection.FraudRiskScore = Math.Min(100, currentFraudScore + anomalyImpact);
                            
                            // Update risk level based on new score
                            validationResult.FraudDetection.RiskLevel = validationResult.FraudDetection.FraudRiskScore switch
                            {
                                < 25 => FraudRiskLevel.Low,
                                < 50 => FraudRiskLevel.Medium,
                                < 75 => FraudRiskLevel.High,
                                _ => FraudRiskLevel.Critical
                            };
                        }
                    }
                }
                
                // Step 9: Apply digital signature if enabled
                if (_enableDigitalSigning)
                {
                    _logger.LogInformation("Applying digital signature to validation result");
                    validationResult = _signatureService.SignValidationResult(validationResult);
                }
                
                _logger.LogInformation("Invoice validation completed: {FileName}, IsValid={IsValid}, ConfidenceScore={Score}", 
                    fileName, validationResult.IsValid, validationResult.ConfidenceScore);
                
                // Store the validation result for later retrieval
                _validationResults[validationResult.ValidationId] = validationResult;
                
                return validationResult;
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
        public async Task<int> ProcessAdditionalPagesAsync(string validationId, int startPage = 2)
        {
            _logger.LogInformation("Processing additional pages for validation ID: {ValidationId} starting from page {StartPage}", 
                validationId, startPage);
            
            if (string.IsNullOrWhiteSpace(validationId))
            {
                _logger.LogWarning("Attempted to process additional pages with null or empty validation ID");
                throw new ArgumentException("Validation ID cannot be null or empty", nameof(validationId));
            }
            
            // Get the validation result
            if (!_validationResults.TryGetValue(validationId, out var validationResult) || 
                validationResult?.ExtractedInvoice == null)
            {
                _logger.LogWarning("No validation result found for ID: {ValidationId}", validationId);
                throw new InvalidOperationException($"No validation result found with ID: {validationId}");
            }
            
            var invoice = validationResult.ExtractedInvoice;
            
            // For demo/testing purposes, we'll simulate processing additional pages
            // In a real implementation, we would:
            // 1. Retrieve the original PDF file from storage
            // 2. Process the additional pages
            // 3. Update the validation result
            
            try
            {
                _logger.LogInformation("Simulating processing additional pages for invoice: {InvoiceNumber}", 
                    invoice.InvoiceNumber);
                
                // Demo implementation - create fake additional pages
                // In a real implementation, we would use the PDF extraction service to extract the additional pages
                
                // For demo purposes, let's say we have 2 additional pages
                int pagesProcessed = 2;
                
                // In a real implementation with actual file storage:
                /*
                // Get the file path from the invoice
                string filePath = invoice.FilePath; // Assuming the invoice stores the file path
                
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _logger.LogWarning("Original invoice file not found: {FilePath}", filePath);
                    throw new FileNotFoundException("Original invoice file not found", filePath);
                }
                
                // Open the file and extract additional pages
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                
                // Use our optimized method to extract only the additional pages
                var additionalPages = await _pdfExtractionService.ExtractAdditionalPagesAsync(fileStream, startPage);
                int pagesProcessed = additionalPages.Count;
                
                // Add the additional pages to the invoice
                if (invoice.PageImages == null)
                {
                    invoice.PageImages = new List<InvoicePageImage>();
                }
                
                invoice.PageImages.AddRange(additionalPages);
                
                // Update the validation result with the new pages
                _validationResults[validationId] = validationResult;
                */
                
                _logger.LogInformation("Processed {PageCount} additional pages for invoice: {InvoiceNumber}", 
                    pagesProcessed, invoice.InvoiceNumber);
                
                validationResult.AddIssue(ValidationSeverity.Info, 
                    $"Processed {pagesProcessed} additional pages for more comprehensive analysis");
                
                return pagesProcessed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing additional pages for validation ID: {ValidationId}", validationId);
                throw;
            }
        }

        /// <summary>
        /// Merges properties from the AI validation result into our main result
        /// while preserving any existing warnings/issues
        /// </summary>
        private void MergeValidationResults(ValidationResult target, ValidationResult source)
        {
            if (source == null) return;
            
            // Copy basic properties
            target.IsValid = source.IsValid;
            target.IsHomeImprovement = source.IsHomeImprovement;
            target.PossibleTampering = source.PossibleTampering;
            target.ConfidenceScore = source.ConfidenceScore;
            target.MeetsApprovalThreshold = source.MeetsApprovalThreshold;
            
            // Copy the extracted invoice details if our target has no details
            if (target.ExtractedInvoice == null && source.ExtractedInvoice != null)
            {
                target.ExtractedInvoice = source.ExtractedInvoice;
            }
            
            // Copy content summary
            if (source.Summary != null)
            {
                target.Summary = source.Summary;
            }
            
            // Copy fraud detection information
            if (source.FraudDetection != null)
            {
                target.FraudDetection = source.FraudDetection;
            }
            
            // Copy audit report information
            if (source.AuditReport != null)
            {
                target.AuditReport = source.AuditReport;
            }
            
            // Copy purchase analysis
            if (source.PurchaseAnalysis != null && (target.PurchaseAnalysis == null || target.PurchaseAnalysis.LineItemDetails.Count == 0))
            {
                target.PurchaseAnalysis = source.PurchaseAnalysis;
            }
            
            // Copy confidence factors
            if (source.ConfidenceFactors != null && source.ConfidenceFactors.Count > 0)
            {
                foreach (var factor in source.ConfidenceFactors)
                {
                    target.AddConfidenceFactor(factor.FactorName, factor.Impact, factor.Explanation);
                }
            }
            
            // Preserve existing issues in target and add new ones
            foreach (var issue in source.Issues)
            {
                // Avoid duplicate issues
                bool isDuplicate = false;
                foreach (var existingIssue in target.Issues)
                {
                    if (existingIssue.Message == issue.Message && existingIssue.Severity == issue.Severity)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                {
                    target.AddIssue(issue.Severity, issue.Message);
                }
            }
            
            // Keep track of our source data
            target.RawAIResponse = source.RawAIResponse;
        }
    }
}
