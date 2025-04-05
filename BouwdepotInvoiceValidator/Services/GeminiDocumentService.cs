using System.Text;
using System.Text.Json.Serialization;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for verifying document types using Gemini API
    /// </summary>
    public class GeminiDocumentService : GeminiServiceBase
    {
        private readonly ILogger<GeminiDocumentService> _documentLogger;
        
        public GeminiDocumentService(ILogger<GeminiDocumentService> logger, IConfiguration configuration) 
            : base(logger, configuration)
        {
            _documentLogger = logger;
        }
        
        /// <summary>
        /// Verifies if a document is an invoice or a different type of document using visual analysis
        /// </summary>
        public async Task<ValidationResult> VerifyDocumentTypeAsync(Invoice invoice)
        {
            _documentLogger.LogInformation("Verifying if document is actually an invoice using VISUAL ONLY analysis: {FileName}", invoice.FileName);
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Make sure we have page images
                if (invoice.PageImages == null || invoice.PageImages.Count == 0)
                {
                    // If we don't have images already, extract them
                    _documentLogger.LogWarning("No page images available for visual document verification. Extracting images first.");
                    
                    // Return a result indicating we can't proceed
                    result.AddIssue(ValidationSeverity.Error, 
                        "Cannot verify document type - no images available for visual analysis.");
                    result.IsValid = false;
                    return result;
                }
                
                // Prepare the visual document analysis prompt
                var prompt = BuildVisualDocumentTypePrompt(invoice);
                
                _documentLogger.LogInformation("Sending {Count} images to Gemini for visual document verification", invoice.PageImages.Count);
                // Call Gemini API with images and specific operation name
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "VisualDocumentTypeVerification");
                result.RawGeminiResponse = response;
                
                // Parse the response
                var documentVerification = ParseDocumentTypeResponse(response, result);
                
                // Set document verification properties in the validation result
                result.IsVerifiedInvoice = documentVerification.IsInvoice;
                result.DetectedDocumentType = documentVerification.DetectedDocumentType;
                result.DocumentVerificationConfidence = documentVerification.Confidence;
                
                if (!documentVerification.IsInvoice)
                {
                    _documentLogger.LogWarning("Document not validated as invoice based on visual analysis. Detected as: {DocumentType}", 
                        documentVerification.DetectedDocumentType);
                    
                    result.AddIssue(ValidationSeverity.Error, 
                        $"The document does not appear to be an invoice based on visual analysis. It looks like a {documentVerification.DetectedDocumentType}.");
                    result.IsValid = false;
                }
                else
                {
                    _documentLogger.LogInformation("Document verified as valid invoice with visual analysis confidence: {Confidence:P0}", 
                        documentVerification.Confidence);
                    
                    result.AddIssue(ValidationSeverity.Info, 
                        "Document verified as a valid invoice based on visual analysis.");
                    result.IsValid = true;
                    
                    // Add confidence info
                    if (documentVerification.Confidence > 0)
                    {
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Invoice visual classification confidence: {documentVerification.Confidence:P0}");
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _documentLogger.LogError(ex, "Error in visual verification of document type with Gemini for {FileName}", invoice.FileName);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error in visual verification of document type: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }
        
        private string BuildVisualDocumentTypePrompt(Invoice invoice)
        {
            _documentLogger.LogDebug("Building visual document type verification prompt for {FileName}", invoice.FileName);
            
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### VISUAL DOCUMENT TYPE VERIFICATION ###");
            promptBuilder.AppendLine("You are an expert in visual document classification and verification.");
            promptBuilder.AppendLine("I am attaching images of a document. Your task is to determine if it is an invoice or a different type of document.");
            promptBuilder.AppendLine("IMPORTANT: Analyze ONLY the VISUAL STRUCTURE and LAYOUT to make this determination.");
            promptBuilder.AppendLine("DO NOT attempt to read the text in detail - focus on the overall visual appearance and structure.");
            
            promptBuilder.AppendLine("\n### INVOICE VISUAL CHARACTERISTICS ###");
            promptBuilder.AppendLine("An invoice typically has the following visual characteristics:");
            promptBuilder.AppendLine("1. Formal header area with company logo/information at the top");
            promptBuilder.AppendLine("2. Clear sectional layout with distinct areas for different information");
            promptBuilder.AppendLine("3. Tabular format in the middle for listing items/services");
            promptBuilder.AppendLine("4. Structured section at the bottom for totals, taxes and payment information");
            promptBuilder.AppendLine("5. Professional, formal business document appearance");
            promptBuilder.AppendLine("6. Often contains visible invoice number and date fields");
            
            promptBuilder.AppendLine("\n### OTHER DOCUMENT TYPES - VISUAL DIFFERENCES ###");
            promptBuilder.AppendLine("- Quotation/Estimate: Similar to invoice but often marked with terms like 'Quotation'/'Offerte'");
            promptBuilder.AppendLine("- Receipt: Typically more compressed, narrower format, often visually simpler than invoices");
            promptBuilder.AppendLine("- Order confirmation: Similar structure to invoice but with order-specific visual elements");
            promptBuilder.AppendLine("- Delivery note: Focus on item details, often with spaces for signatures or checkboxes");
            promptBuilder.AppendLine("- Statement: Often has a running list of transactions with dates in chronological order");
            promptBuilder.AppendLine("- Contract/Agreement: Usually dense text, often with signature lines at bottom, formal paragraph structure");
            promptBuilder.AppendLine("- Letter: Typically has formal letter layout with address block and salutation");
            
            promptBuilder.AppendLine("\n### VISUAL ANALYSIS INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Analyze the overall document structure and layout");
            promptBuilder.AppendLine("2. Look for the presence of invoice-specific structural elements");
            promptBuilder.AppendLine("3. Check how information appears to be organized visually");
            promptBuilder.AppendLine("4. Compare the visual patterns to known document types");
            promptBuilder.AppendLine("5. Assign a confidence level based on visual invoice characteristics");
            
            promptBuilder.AppendLine("\n### IMPORTANT REMINDER ###");
            promptBuilder.AppendLine("Focus ONLY on what you can determine from the VISUAL LAYOUT. DO NOT try to read small text.");
            promptBuilder.AppendLine("Consider how the document is structured visually rather than specific content.");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isInvoice\": true/false,");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"detectedDocumentType\": \"Invoice or the alternative document type identified\",");
            promptBuilder.AppendLine("  \"reasoning\": \"Your detailed visual analysis and reasoning\",");
            promptBuilder.AppendLine("  \"presentInvoiceElements\": [\"List of visually-identifiable invoice elements present\"],");
            promptBuilder.AppendLine("  \"missingInvoiceElements\": [\"List of standard invoice visual elements that are missing\"]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        private string BuildDocumentTypePrompt(Invoice invoice)
        {
            _documentLogger.LogDebug("Building document type verification prompt for {FileName}", invoice.FileName);
            
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### DOCUMENT TYPE VERIFICATION ###");
            promptBuilder.AppendLine("You are an expert in document classification and verification.");
            promptBuilder.AppendLine("Your task is to determine if the provided document is actually an invoice or a different type of document.");
            promptBuilder.AppendLine("Analyze both the VISUAL STRUCTURE and TEXTUAL CONTENT to make this determination.");
            
            promptBuilder.AppendLine("\n### INVOICE CHARACTERISTICS ###");
            promptBuilder.AppendLine("An invoice typically contains the following elements:");
            promptBuilder.AppendLine("1. The word 'Invoice' or 'Factuur' in the header or title");
            promptBuilder.AppendLine("2. Invoice number and date");
            promptBuilder.AppendLine("3. Vendor details (name, address, contact information)");
            promptBuilder.AppendLine("4. Customer/billing information");
            promptBuilder.AppendLine("5. Itemized list of products or services with quantities and prices");
            promptBuilder.AppendLine("6. Total amount due");
            promptBuilder.AppendLine("7. Payment terms and methods");
            promptBuilder.AppendLine("8. Tax information (VAT/BTW numbers, tax calculations)");
            
            promptBuilder.AppendLine("\n### OTHER DOCUMENT TYPES TO CHECK ###");
            promptBuilder.AppendLine("This might be another type of document such as:");
            promptBuilder.AppendLine("- Quotation/Estimate ('Offerte'): Future-dated, contains words like 'estimate', 'quotation', 'offerte'");
            promptBuilder.AppendLine("- Receipt ('Bon', 'Kwitantie'): Usually simpler than invoices, often for retail purchases");
            promptBuilder.AppendLine("- Order confirmation: Contains words like 'order', 'bestelling', 'confirmation'");
            promptBuilder.AppendLine("- Delivery note ('Pakbon'): Focused on delivered items, not payment");
            promptBuilder.AppendLine("- Statement ('Overzicht'): Summary of multiple transactions");
            promptBuilder.AppendLine("- Contract or agreement: Legal document, not a payment request");
            promptBuilder.AppendLine("- Letter or general correspondence: Lacks invoice structure");
            
            promptBuilder.AppendLine("\n### ANALYSIS INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Examine the document's title and header information");
            promptBuilder.AppendLine("2. Check for the presence of invoice-specific elements");
            promptBuilder.AppendLine("3. Look for conflicting terms that suggest another document type");
            promptBuilder.AppendLine("4. Analyze the overall structure and purpose of the document");
            promptBuilder.AppendLine("5. Determine confidence level based on how many invoice characteristics are present");
            
            promptBuilder.AppendLine("\n### CONTENT TO ANALYZE ###");
            promptBuilder.AppendLine($"Document filename: {invoice.FileName}");
            promptBuilder.AppendLine($"Extracted text snippet (beginning): {invoice.RawText.Substring(0, Math.Min(500, invoice.RawText.Length))}");
            
            // Add extracted invoice data for additional context
            if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                promptBuilder.AppendLine($"Extracted invoice number: {invoice.InvoiceNumber}");
            }
            
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Extracted date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
            }
            
            if (invoice.TotalAmount > 0)
            {
                promptBuilder.AppendLine($"Extracted total amount: {invoice.TotalAmount:C}");
            }
            
            if (!string.IsNullOrEmpty(invoice.VendorName))
            {
                promptBuilder.AppendLine($"Extracted vendor name: {invoice.VendorName}");
            }
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isInvoice\": true/false,");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"detectedDocumentType\": \"Invoice or the alternative document type identified\",");
            promptBuilder.AppendLine("  \"reasoning\": \"Your detailed analysis and reasoning\",");
            promptBuilder.AppendLine("  \"presentInvoiceElements\": [\"List of invoice elements present in the document\"],");
            promptBuilder.AppendLine("  \"missingInvoiceElements\": [\"List of standard invoice elements that are missing\"]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        private DocumentTypeVerification ParseDocumentTypeResponse(string responseText, ValidationResult result)
        {
            _documentLogger.LogDebug("Parsing document type verification response");
            
            var verification = new DocumentTypeVerification
            {
                IsInvoice = true, // Default to true to avoid false negatives
                Confidence = 0.75,
                DetectedDocumentType = "Invoice"
            };
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<DocumentTypeResponse>(responseText);
                
                if (response != null)
                {
                    verification.IsInvoice = response.IsInvoice;
                    verification.Confidence = response.Confidence;
                    verification.DetectedDocumentType = response.DetectedDocumentType;
                    
                    _documentLogger.LogInformation("Document type analysis: IsInvoice={IsInvoice}, Confidence={Confidence:P0}, Type={Type}", 
                        response.IsInvoice, response.Confidence, response.DetectedDocumentType);
                    
                    // Add present and missing elements to the validation result
                    if (response.PresentInvoiceElements.Count > 0)
                    {
                        result.PresentInvoiceElements = response.PresentInvoiceElements;
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Invoice elements present: {string.Join(", ", response.PresentInvoiceElements)}");
                            
                        _documentLogger.LogDebug("Present invoice elements: {Elements}", 
                            string.Join(", ", response.PresentInvoiceElements));
                    }
                    
                    if (response.MissingInvoiceElements.Count > 0)
                    {
                        result.MissingInvoiceElements = response.MissingInvoiceElements;
                        result.AddIssue(ValidationSeverity.Warning, 
                            $"Invoice elements missing: {string.Join(", ", response.MissingInvoiceElements)}");
                            
                        _documentLogger.LogDebug("Missing invoice elements: {Elements}", 
                            string.Join(", ", response.MissingInvoiceElements));
                    }
                    
                    // Add reasoning as info
                    result.AddIssue(ValidationSeverity.Info, 
                        $"Document type analysis: {response.Reasoning}");
                }
                else
                {
                    _documentLogger.LogWarning("Failed to parse document type response as JSON, performing keyword analysis");
                    
                    // If we can't parse JSON, check for keywords
                    var isInvoice = responseText.Contains("is an invoice", StringComparison.OrdinalIgnoreCase) || 
                                  responseText.Contains("valid invoice", StringComparison.OrdinalIgnoreCase);
                    
                    var isNotInvoice = responseText.Contains("not an invoice", StringComparison.OrdinalIgnoreCase) || 
                                     responseText.Contains("different document type", StringComparison.OrdinalIgnoreCase);
                    
                    if (isNotInvoice)
                    {
                        verification.IsInvoice = false;
                        verification.DetectedDocumentType = DetectDocumentTypeFromText(responseText);
                        
                        _documentLogger.LogWarning("Keyword analysis detected document is not an invoice, but a {DocumentType}", 
                            verification.DetectedDocumentType);
                            
                        result.AddIssue(ValidationSeverity.Error, 
                            $"Document appears to be a {verification.DetectedDocumentType}, not an invoice.");
                    }
                }
            }
            catch (Exception ex)
            {
                _documentLogger.LogError(ex, "Error parsing document type verification response: {ErrorMessage}", ex.Message);
                // Use default values (true) to avoid false negatives
            }
            
            return verification;
        }
        
        private string DetectDocumentTypeFromText(string text)
        {
            _documentLogger.LogDebug("Detecting document type from text using keyword analysis");
            
            // Check for invoice-related keywords first
            if (text.Contains("invoice", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("factuur", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("rekening", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("facture", StringComparison.OrdinalIgnoreCase))
                return "Invoice";
                
            // Check for invoice-related patterns
            if ((text.Contains("btw", StringComparison.OrdinalIgnoreCase) && text.Contains("totaal", StringComparison.OrdinalIgnoreCase)) ||
                (text.Contains("vat", StringComparison.OrdinalIgnoreCase) && text.Contains("total", StringComparison.OrdinalIgnoreCase)) ||
                (text.Contains("betaling", StringComparison.OrdinalIgnoreCase) && text.Contains("datum", StringComparison.OrdinalIgnoreCase)) ||
                (text.Contains("payment", StringComparison.OrdinalIgnoreCase) && text.Contains("date", StringComparison.OrdinalIgnoreCase)))
                return "Invoice";
            
            // Simple detection of alternative document types from text
            if (text.Contains("quotation", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("offerte", StringComparison.OrdinalIgnoreCase))
                return "Quotation/Estimate";
            
            if (text.Contains("receipt", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("bon", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("kwitantie", StringComparison.OrdinalIgnoreCase))
                return "Receipt";
            
            if (text.Contains("order confirmation", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("orderbevestiging", StringComparison.OrdinalIgnoreCase))
                return "Order Confirmation";
            
            if (text.Contains("delivery note", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("pakbon", StringComparison.OrdinalIgnoreCase))
                return "Delivery Note";
            
            if (text.Contains("statement", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("overzicht", StringComparison.OrdinalIgnoreCase))
                return "Statement";
            
            if (text.Contains("contract", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("agreement", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("overeenkomst", StringComparison.OrdinalIgnoreCase))
                return "Contract/Agreement";
            
            // Default to Invoice for any document that contains financial information
            if (text.Contains("euro", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("€", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("bedrag", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("amount", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("prijs", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("price", StringComparison.OrdinalIgnoreCase))
                return "Invoice";
            
            // Fall back to a more specific unknown type
            return "Unrecognized Document (Likely Invoice)";
        }
        
        internal class DocumentTypeVerification
        {
            public bool IsInvoice { get; set; }
            public double Confidence { get; set; }
            public string DetectedDocumentType { get; set; } = string.Empty;
        }
        
        internal class DocumentTypeResponse
        {
            [JsonPropertyName("isInvoice")]
            public bool IsInvoice { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("detectedDocumentType")]
            public string DetectedDocumentType { get; set; } = string.Empty;
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
            
            [JsonPropertyName("presentInvoiceElements")]
            public List<string> PresentInvoiceElements { get; set; } = new List<string>();
            
            [JsonPropertyName("missingInvoiceElements")]
            public List<string> MissingInvoiceElements { get; set; } = new List<string>();
        }
    }
}
