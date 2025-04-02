using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Gemini-specific implementation of the AI model provider interface
    /// Adapts the unified validation approach for Gemini's API
    /// </summary>
    public class GeminiModelProvider : AIModelProviderBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiModelProvider> _logger;
        private readonly GeminiServiceBase _geminiService;
        
        public GeminiModelProvider(
            ILogger<GeminiModelProvider> logger,
            IConfiguration configuration,
            GeminiServiceBase geminiService) : base(logger)
        {
            _logger = logger;
            _configuration = configuration;
            _geminiService = geminiService;
        }
        
        /// <inheritdoc />
        public override ModelProviderInfo GetProviderInfo()
        {
            return new ModelProviderInfo
            {
                ProviderName = "Google Gemini",
                ModelId = "gemini-pro",
                ModelVersion = "1.0",
                MaxContextLength = 30000,
                SupportsImages = true,
                Capabilities = new List<string>
                {
                    "Text Analysis",
                    "Image Analysis",
                    "Invoice Validation",
                    "Data Extraction",
                    "Multi-modal Analysis"
                }
            };
        }
        
        /// <inheritdoc />
        public override async Task<InvoiceValidationResponse> ValidateInvoiceAsync(Invoice invoice, ValidationOptions options = null)
        {
            options ??= new ValidationOptions();
            _logger.LogInformation("Validating invoice with Gemini using unified approach");
            
            try
            {
                // Build the unified prompt
                string prompt = BuildUnifiedValidationPrompt(invoice, options);
                
                // Call Gemini API
                string response;
                if (options.IncludeVisualAnalysis && invoice.PageImages != null && invoice.PageImages.Count > 0)
                {
                    _logger.LogInformation("Including visual analysis with {ImageCount} page images", invoice.PageImages.Count);
                    response = await _geminiService.CallGeminiApiAsync(prompt, invoice.PageImages, "UnifiedInvoiceValidation");
                }
                else
                {
                    _logger.LogInformation("Performing text-only analysis");
                    response = await _geminiService.CallGeminiApiAsync(prompt, null, "UnifiedInvoiceValidation");
                }
                
                // Parse the response into our standard format
                return ParseValidationResponse(response, invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice with Gemini");
                
                // Create error response
                var errorResponse = new InvoiceValidationResponse
                {
                    IsValidInvoice = false,
                    ConfidenceScore = 0,
                    ExtractedInvoice = invoice,
                    ValidationNotes = new List<ValidationNote>
                    {
                        new ValidationNote
                        {
                            Severity = ValidationSeverity.Error,
                            Message = $"Error validating invoice: {ex.Message}",
                            Section = "General"
                        }
                    },
                    RawResponse = ex.ToString()
                };
                
                return errorResponse;
            }
        }
        
        /// <inheritdoc />
        public override async Task<Invoice> ExtractDataFromInvoiceAsync(Invoice invoice)
        {
            _logger.LogInformation("Extracting data from invoice with Gemini");
            
            try
            {
                // Build data extraction prompt
                string prompt = BuildDataExtractionPrompt(invoice);
                
                // Call Gemini API with images if available
                string response;
                if (invoice.PageImages != null && invoice.PageImages.Count > 0)
                {
                    _logger.LogInformation("Using visual data extraction with {ImageCount} page images", invoice.PageImages.Count);
                    response = await _geminiService.CallGeminiApiAsync(prompt, invoice.PageImages, "InvoiceDataExtraction");
                }
                else
                {
                    _logger.LogInformation("Using text-only data extraction");
                    response = await _geminiService.CallGeminiApiAsync(prompt, null, "InvoiceDataExtraction");
                }
                
                // Parse the response and update the invoice
                return ParseDataExtractionResponse(response, invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting data from invoice with Gemini");
                // Return original invoice to avoid data loss
                return invoice;
            }
        }
        
        /// <summary>
        /// Builds a prompt for extracting data from an invoice
        /// </summary>
        private string BuildDataExtractionPrompt(Invoice invoice)
        {
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### INVOICE DATA EXTRACTION ###");
            promptBuilder.AppendLine("You are an invoice data extraction specialist trained to extract key information from invoices.");
            promptBuilder.AppendLine("Extract the following information:");
            promptBuilder.AppendLine("1. Invoice number");
            promptBuilder.AppendLine("2. Invoice date (format as YYYY-MM-DD)");
            promptBuilder.AppendLine("3. Total amount (numeric value only)");
            promptBuilder.AppendLine("4. VAT amount (if present)");
            promptBuilder.AppendLine("5. Vendor name");
            promptBuilder.AppendLine("6. Vendor address");
            promptBuilder.AppendLine("7. Vendor KvK number (Dutch Chamber of Commerce number, if present)");
            promptBuilder.AppendLine("8. Vendor BTW number (Dutch VAT number, if present)");
            promptBuilder.AppendLine("9. Line items (descriptions, quantities, prices)");
            
            // Dutch terminology help
            promptBuilder.AppendLine("\n### DUTCH TERMINOLOGY ###");
            promptBuilder.AppendLine("- 'Factuur'/'Nota' = Invoice");
            promptBuilder.AppendLine("- 'Factuurdatum' = Invoice date");
            promptBuilder.AppendLine("- 'Factuurnummer' = Invoice number");
            promptBuilder.AppendLine("- 'Totaalbedrag'/'Totaal' = Total amount");
            promptBuilder.AppendLine("- 'BTW'/'BTW-nummer' = VAT/VAT Number");
            promptBuilder.AppendLine("- 'KvK'/'KvK-nummer' = Chamber of Commerce number");
            
            // Output format
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in this format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"invoiceNumber\": \"string\",");
            promptBuilder.AppendLine("  \"invoiceDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("  \"totalAmount\": number,");
            promptBuilder.AppendLine("  \"vatAmount\": number,");
            promptBuilder.AppendLine("  \"vendorName\": \"string\",");
            promptBuilder.AppendLine("  \"vendorAddress\": \"string\",");
            promptBuilder.AppendLine("  \"vendorKvkNumber\": \"string\",");
            promptBuilder.AppendLine("  \"vendorBtwNumber\": \"string\",");
            promptBuilder.AppendLine("  \"lineItems\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"string\",");
            promptBuilder.AppendLine("      \"quantity\": number,");
            promptBuilder.AppendLine("      \"unitPrice\": number,");
            promptBuilder.AppendLine("      \"totalPrice\": number,");
            promptBuilder.AppendLine("      \"vatRate\": number");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        /// <summary>
        /// Parses the response from data extraction
        /// </summary>
        private Invoice ParseDataExtractionResponse(string responseText, Invoice invoice)
        {
            try
            {
                // Extract and sanitize JSON
                string jsonText = ExtractJsonFromResponse(responseText);
                jsonText = SanitizeJsonString(jsonText);
                
                // Deserialize
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var extractionResult = JsonSerializer.Deserialize<ExtractionResponse>(jsonText, options);
                
                if (extractionResult != null)
                {
                    // Update invoice with extracted data
                    invoice.InvoiceNumber = extractionResult.InvoiceNumber ?? invoice.InvoiceNumber;
                    
                    // Parse date with proper handling
                    if (!string.IsNullOrEmpty(extractionResult.InvoiceDate))
                    {
                        if (DateTime.TryParse(extractionResult.InvoiceDate, out DateTime date))
                        {
                            invoice.InvoiceDate = date;
                        }
                    }
                    
                    // Set amounts
                    if (extractionResult.TotalAmount > 0)
                    {
                        invoice.TotalAmount = extractionResult.TotalAmount;
                    }
                    
                    if (extractionResult.VatAmount > 0)
                    {
                        invoice.VatAmount = extractionResult.VatAmount;
                    }
                    
                    // Set vendor details
                    if (!string.IsNullOrEmpty(extractionResult.VendorName))
                    {
                        invoice.VendorName = extractionResult.VendorName;
                    }
                    
                    if (!string.IsNullOrEmpty(extractionResult.VendorAddress))
                    {
                        invoice.VendorAddress = extractionResult.VendorAddress;
                    }
                    
                    if (!string.IsNullOrEmpty(extractionResult.VendorKvkNumber))
                    {
                        invoice.VendorKvkNumber = extractionResult.VendorKvkNumber;
                    }
                    
                    if (!string.IsNullOrEmpty(extractionResult.VendorBtwNumber))
                    {
                        invoice.VendorBtwNumber = extractionResult.VendorBtwNumber;
                    }
                    
                    // Add line items
                    if (extractionResult.LineItems != null && extractionResult.LineItems.Count > 0)
                    {
                        // Clear existing items only if we got new ones
                        invoice.LineItems.Clear();
                        
                        foreach (var item in extractionResult.LineItems)
                        {
                            var lineItem = new InvoiceLineItem
                            {
                                Description = item.Description ?? "",
                                Quantity = item.Quantity > 0 ? item.Quantity : 1,
                                UnitPrice = item.UnitPrice,
                                TotalPrice = item.TotalPrice > 0 ? item.TotalPrice : (item.UnitPrice * item.Quantity),
                                VatRate = item.VatRate > 0 ? item.VatRate : 21 // Default Dutch high rate
                            };
                            
                            invoice.LineItems.Add(lineItem);
                        }
                    }
                    
                    _logger.LogInformation("Successfully extracted invoice data: {InvoiceNumber}, {Date}, {Amount}",
                        invoice.InvoiceNumber, invoice.InvoiceDate, invoice.TotalAmount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing data extraction response");
                // Return partially updated invoice
            }
            
            return invoice;
        }
        
        /// <summary>
        /// Parses the validation response into a standardized format
        /// </summary>
        private InvoiceValidationResponse ParseValidationResponse(string responseText, Invoice invoice)
        {
            var result = new InvoiceValidationResponse
            {
                ExtractedInvoice = invoice,
                RawResponse = responseText
            };
            
            try
            {
                // Extract and sanitize JSON
                string jsonText = ExtractJsonFromResponse(responseText);
                jsonText = SanitizeJsonString(jsonText);
                
                // Deserialize
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var validationResult = JsonSerializer.Deserialize<ValidationResponse>(jsonText, options);
                
                if (validationResult != null)
                {
                    // Set core validation results
                    result.IsValidInvoice = validationResult.IsValidInvoice;
                    result.IsHomeImprovement = validationResult.IsHomeImprovement;
                    result.ConfidenceScore = validationResult.ConfidenceScore;
                    result.Summary = validationResult.Summary;
                    result.PrimaryPurpose = validationResult.PrimaryPurpose;
                    result.Categories = validationResult.Categories;
                    result.AuditJustification = validationResult.AuditJustification;
                    
                    // Update extracted invoice data if provided
                    if (validationResult.ExtractedData != null)
                    {
                        UpdateInvoiceFromExtractedData(invoice, validationResult.ExtractedData);
                    }
                    
                    // Set line item analysis
                    if (validationResult.LineItems != null && validationResult.LineItems.Count > 0)
                    {
                        foreach (var item in validationResult.LineItems)
                        {
                            result.LineItemAnalysis.Add(new LineItemValidation
                            {
                                Description = item.Description,
                                InterpretedAs = item.InterpretedAs,
                                Category = item.Category,
                                IsEligible = item.IsEligible,
                                IsPermanentlyAttached = item.IsPermanentlyAttached,
                                ImproveHomeQuality = item.ImprovesHomeQuality,
                                Confidence = item.Confidence,
                                Notes = item.Notes
                            });
                        }
                    }
                    
                    // Set validation notes
                    if (validationResult.ValidationNotes != null && validationResult.ValidationNotes.Count > 0)
                    {
                        foreach (var note in validationResult.ValidationNotes)
                        {
                            result.ValidationNotes.Add(new ValidationNote
                            {
                                Severity = ParseSeverity(note.Severity),
                                Message = note.Message,
                                Section = note.Section
                            });
                        }
                    }
                    
                    // Set fraud indicators
                    if (validationResult.FraudIndicators != null && validationResult.FraudIndicators.Count > 0)
                    {
                        foreach (var indicator in validationResult.FraudIndicators)
                        {
                            result.FraudIndicators.Add(new FraudIndicator
                            {
                                IndicatorName = indicator.Indicator,
                                Description = indicator.Description,
                                Evidence = indicator.Evidence,
                                Severity = indicator.Severity,
                                Category = FraudIndicatorCategory.DocumentIssue // Default category
                            });
                        }
                    }
                    
                    // Set criteria assessments
                    if (validationResult.CriteriaAssessments != null && validationResult.CriteriaAssessments.Count > 0)
                    {
                        foreach (var assessment in validationResult.CriteriaAssessments)
                        {
                            result.CriteriaAssessments.Add(new CriterionAssessment
                            {
                                CriterionName = assessment.CriterionName,
                                Score = assessment.Score,
                                Evidence = assessment.Evidence,
                                Reasoning = assessment.Reasoning
                            });
                        }
                    }
                    
                    // Set Bouwdepot validation
                    if (validationResult.BouwdepotValidation != null)
                    {
                        result.BouwdepotValidation = new BouwdepotValidationDetails
                        {
                            QualityImprovementRule = validationResult.BouwdepotValidation.QualityImprovementRule,
                            PermanentAttachmentRule = validationResult.BouwdepotValidation.PermanentAttachmentRule,
                            GeneralValidationNotes = validationResult.BouwdepotValidation.GeneralValidationNotes
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize validation response");
                    result.ValidationNotes.Add(new ValidationNote
                    {
                        Severity = ValidationSeverity.Error,
                        Message = "Failed to parse AI model response",
                        Section = "General"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing validation response: {ErrorMessage}", ex.Message);
                result.ValidationNotes.Add(new ValidationNote
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Error processing validation: {ex.Message}",
                    Section = "General"
                });
            }
            
            return result;
        }
        
        /// <summary>
        /// Updates invoice with extracted data
        /// </summary>
        private void UpdateInvoiceFromExtractedData(Invoice invoice, ExtractedData data)
        {
            if (data == null) return;
            
            // Update only if we have valid data
            if (!string.IsNullOrEmpty(data.InvoiceNumber))
            {
                invoice.InvoiceNumber = data.InvoiceNumber;
            }
            
            if (!string.IsNullOrEmpty(data.InvoiceDate))
            {
                if (DateTime.TryParse(data.InvoiceDate, out DateTime date))
                {
                    invoice.InvoiceDate = date;
                }
            }
            
            if (data.TotalAmount > 0)
            {
                invoice.TotalAmount = data.TotalAmount;
            }
            
            if (!string.IsNullOrEmpty(data.VendorName))
            {
                invoice.VendorName = data.VendorName;
            }
            
            if (!string.IsNullOrEmpty(data.VendorAddress))
            {
                invoice.VendorAddress = data.VendorAddress;
            }
            
            if (!string.IsNullOrEmpty(data.VendorKvkNumber))
            {
                invoice.VendorKvkNumber = data.VendorKvkNumber;
            }
            
            if (!string.IsNullOrEmpty(data.VendorBtwNumber))
            {
                invoice.VendorBtwNumber = data.VendorBtwNumber;
            }
        }
        
        /// <summary>
        /// Parses severity string to enum
        /// </summary>
        private ValidationSeverity ParseSeverity(string severity)
        {
            if (string.IsNullOrEmpty(severity))
                return ValidationSeverity.Info;
                
            switch (severity.ToLowerInvariant())
            {
                case "error":
                    return ValidationSeverity.Error;
                case "warning":
                    return ValidationSeverity.Warning;
                case "info":
                default:
                    return ValidationSeverity.Info;
            }
        }
        
        #region Response Models
        
        /// <summary>
        /// Data extraction response model
        /// </summary>
        private class ExtractionResponse
        {
            public string InvoiceNumber { get; set; }
            public string InvoiceDate { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal VatAmount { get; set; }
            public string VendorName { get; set; }
            public string VendorAddress { get; set; }
            public string VendorKvkNumber { get; set; }
            public string VendorBtwNumber { get; set; }
            public List<ExtractionLineItem> LineItems { get; set; } = new List<ExtractionLineItem>();
        }
        
        /// <summary>
        /// Line item in extraction response
        /// </summary>
        private class ExtractionLineItem
        {
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal VatRate { get; set; }
        }
        
        /// <summary>
        /// Full validation response model
        /// </summary>
        private class ValidationResponse
        {
            public bool IsValidInvoice { get; set; }
            public bool IsHomeImprovement { get; set; }
            public int ConfidenceScore { get; set; }
            public ExtractedData ExtractedData { get; set; }
            public List<ValidationLineItem> LineItems { get; set; } = new List<ValidationLineItem>();
            public List<string> Categories { get; set; } = new List<string>();
            public string PrimaryPurpose { get; set; }
            public string Summary { get; set; }
            public List<ValidationNoteResponse> ValidationNotes { get; set; } = new List<ValidationNoteResponse>();
            public List<FraudIndicatorResponse> FraudIndicators { get; set; } = new List<FraudIndicatorResponse>();
            public string AuditJustification { get; set; }
            public List<CriteriaAssessmentResponse> CriteriaAssessments { get; set; } = new List<CriteriaAssessmentResponse>();
            public BouwdepotValidationResponse BouwdepotValidation { get; set; }
        }
        
        /// <summary>
        /// Extracted data in validation response
        /// </summary>
        private class ExtractedData
        {
            public string InvoiceNumber { get; set; }
            public string InvoiceDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string VendorName { get; set; }
            public string VendorAddress { get; set; }
            public string VendorKvkNumber { get; set; }
            public string VendorBtwNumber { get; set; }
        }
        
        /// <summary>
        /// Line item in validation response
        /// </summary>
        private class ValidationLineItem
        {
            public string Description { get; set; }
            public string InterpretedAs { get; set; }
            public string Category { get; set; }
            public bool IsEligible { get; set; }
            public bool IsPermanentlyAttached { get; set; }
            public bool ImprovesHomeQuality { get; set; }
            public int Confidence { get; set; }
            public string Notes { get; set; }
        }
        
        /// <summary>
        /// Validation note in response
        /// </summary>
        private class ValidationNoteResponse
        {
            public string Severity { get; set; }
            public string Message { get; set; }
            public string Section { get; set; }
        }
        
        /// <summary>
        /// Fraud indicator in response
        /// </summary>
        private class FraudIndicatorResponse
        {
            public string Indicator { get; set; }
            public string Description { get; set; }
            public string Evidence { get; set; }
            public double Severity { get; set; }
        }
        
        /// <summary>
        /// Criteria assessment in response
        /// </summary>
        private class CriteriaAssessmentResponse
        {
            public string CriterionName { get; set; }
            public int Score { get; set; }
            public string Evidence { get; set; }
            public string Reasoning { get; set; }
        }
        
        /// <summary>
        /// Bouwdepot validation in response
        /// </summary>
        private class BouwdepotValidationResponse
        {
            public bool QualityImprovementRule { get; set; }
            public bool PermanentAttachmentRule { get; set; }
            public string GeneralValidationNotes { get; set; }
        }
        
        #endregion
    }
}
