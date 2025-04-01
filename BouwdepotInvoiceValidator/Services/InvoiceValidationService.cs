using System;
using System.IO;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for validating invoice PDFs
    /// </summary>
    public class InvoiceValidationService : IInvoiceValidationService
    {
        private readonly ILogger<InvoiceValidationService> _logger;
        private readonly IPdfExtractionService _pdfExtractionService;
        private readonly IGeminiService _geminiService;

        public InvoiceValidationService(
            ILogger<InvoiceValidationService> logger,
            IPdfExtractionService pdfExtractionService,
            IGeminiService geminiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pdfExtractionService = pdfExtractionService ?? throw new ArgumentNullException(nameof(pdfExtractionService));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
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
                
                // Check if we could extract the basic invoice data
                if (string.IsNullOrEmpty(invoice.InvoiceNumber) || !invoice.InvoiceDate.HasValue || invoice.TotalAmount <= 0)
                {
                    _logger.LogWarning("Failed to extract important invoice data from {FileName}", fileName);
                    
                    var extractionFailResult = new ValidationResult
                    {
                        IsValid = false,
                        ExtractedInvoice = invoice
                    };
                    
                    extractionFailResult.AddIssue(ValidationSeverity.Error, 
                        "Failed to extract critical invoice information. Please check the file quality.");
                    
                    return extractionFailResult;
                }
                
                // Step 3: Validate with Gemini
                _logger.LogInformation("Validating invoice with Gemini AI");
                var validationResult = await _geminiService.ValidateHomeImprovementAsync(invoice);
                
                // Step 4: Check for fraud indicators using Gemini
                if (validationResult.IsValid)
                {
                    _logger.LogInformation("Checking for fraud indicators");
                    bool possibleFraud = await _geminiService.DetectFraudAsync(invoice, possibleTampering);
                    
                    if (possibleFraud)
                    {
                        _logger.LogWarning("Possible fraud detected in invoice {FileName}", fileName);
                        validationResult.IsValid = false;
                        validationResult.PossibleTampering = true;
                        validationResult.AddIssue(ValidationSeverity.Error, 
                            "Possible fraud detected in this invoice. Please review manually.");
                    }
                }
                
                _logger.LogInformation("Invoice validation completed: {FileName}, IsValid={IsValid}", 
                    fileName, validationResult.IsValid);
                
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
    }
}
