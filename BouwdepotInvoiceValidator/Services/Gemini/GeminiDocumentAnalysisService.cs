using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for analyzing and extracting data from invoice documents using Gemini AI
    /// </summary>
    public class GeminiDocumentAnalysisService : GeminiServiceBase
    {
        public GeminiDocumentAnalysisService(ILogger<GeminiDocumentAnalysisService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }

        /// <summary>
        /// Uses Gemini AI to extract invoice data from the PDF images
        /// </summary>
        /// <param name="invoice">The invoice with page images</param>
        /// <returns>The invoice with extracted data from the images</returns>
        public async Task<Invoice> ExtractInvoiceDataFromImagesAsync(Invoice invoice)
        {
            _logger.LogInformation("Extracting invoice data from images using Gemini AI");
            
            try
            {
                // Make sure we have page images
                if (invoice.PageImages == null || invoice.PageImages.Count == 0)
                {
                    _logger.LogWarning("Invoice data extraction failed: No page images available");
                    return invoice;
                }
                
                // Extract Header Data
                _logger.LogDebug("Extracting invoice header data...");
                var headerPrompt = BuildInvoiceHeaderPrompt();
                var headerResponse = await CallGeminiApiAsync(headerPrompt, invoice.PageImages, "InvoiceHeaderExtraction");
                ParseInvoiceHeaderResponse(headerResponse, invoice);
                _logger.LogDebug("Invoice header data extracted.");

                // Extract Parties Data
                _logger.LogDebug("Extracting invoice parties data...");
                var partiesPrompt = BuildInvoicePartiesPrompt();
                var partiesResponse = await CallGeminiApiAsync(partiesPrompt, invoice.PageImages, "InvoicePartiesExtraction");
                ParseInvoicePartiesResponse(partiesResponse, invoice);
                 _logger.LogDebug("Invoice parties data extracted.");

                // Extract Line Items Data
                _logger.LogDebug("Extracting invoice line items data...");
                var lineItemsPrompt = BuildInvoiceLineItemsPrompt();
                var lineItemsResponse = await CallGeminiApiAsync(lineItemsPrompt, invoice.PageImages, "InvoiceLineItemsExtraction");
                ParseInvoiceLineItemsResponse(lineItemsResponse, invoice);
                _logger.LogDebug("Invoice line items data extracted.");

                _logger.LogInformation("Successfully extracted and merged invoice data using Gemini AI: " +
                                      "InvoiceNumber={InvoiceNumber}, InvoiceDate={InvoiceDate}, TotalAmount={TotalAmount}, Vendor={VendorName}, LineItems={LineItemCount}", 
                                      invoice.InvoiceNumber, invoice.InvoiceDate, invoice.TotalAmount, invoice.VendorName, invoice.LineItems?.Count ?? 0);
                
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice data from images with Gemini");
                // Return the original invoice without modifications in case of error
                return invoice;
            }
        }

        /// <summary>
        /// Uses Gemini AI to verify if the document is actually an invoice
        /// </summary>
        /// <param name="invoice">The extracted document data</param>
        /// <returns>A validation result with Gemini's document type assessment</returns>
        public async Task<ValidationResult> VerifyDocumentTypeAsync(Invoice invoice)
        {
            _logger.LogInformation("Verifying if document is actually an invoice");
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Prepare the document data for Gemini
                var prompt = BuildDocumentTypePrompt(invoice);
                
                // Call Gemini API - explicitly pass null for images
                var response = await CallGeminiApiAsync(prompt, null, "DocumentTypeVerification");
                result.RawGeminiResponse = response;
                
                // Parse the response
                var documentVerification = ParseDocumentTypeResponse(response, result);
                
                // Set document verification properties in the validation result
                result.IsVerifiedInvoice = documentVerification.IsInvoice;
                result.DetectedDocumentType = documentVerification.DetectedDocumentType;
                result.DocumentVerificationConfidence = documentVerification.Confidence;
                
                if (!documentVerification.IsInvoice)
                {
                    result.AddIssue(ValidationSeverity.Error, 
                        $"The document does not appear to be an invoice. It looks like a {documentVerification.DetectedDocumentType}.");
                    result.IsValid = false;
                }
                else
                {
                    result.AddIssue(ValidationSeverity.Info, 
                        "Document verified as a valid invoice.");
                    result.IsValid = true;
                    
                    // Add confidence info
                    if (documentVerification.Confidence > 0)
                    {
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Invoice classification confidence: {documentVerification.Confidence:P0}");
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document type with Gemini");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error verifying document type: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        #region Helper Methods

        // Removed BuildInvoiceDataExtractionPrompt as it's replaced by the three specific prompts below

        private string BuildInvoiceHeaderPrompt()
        {
            var promptBuilder = new System.Text.StringBuilder();

            promptBuilder.AppendLine("### INVOICE HEADER EXTRACTION ###");
            promptBuilder.AppendLine("You are an expert in extracting structured data from invoice images.");
            promptBuilder.AppendLine("Your task is to analyze the provided invoice images and extract key invoice header information.");

            promptBuilder.AppendLine("\n### EXTRACTION INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Extract the following invoice details:");
            promptBuilder.AppendLine("   - Invoice number");
            promptBuilder.AppendLine("   - Invoice date (format as YYYY-MM-DD)");
            promptBuilder.AppendLine("   - Due date (format as YYYY-MM-DD)");
            promptBuilder.AppendLine("   - Total amount (numeric value only)");
            promptBuilder.AppendLine("   - Tax/VAT amount (numeric value only)");
            promptBuilder.AppendLine("   - Currency");

            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"invoiceNumber\": \"The invoice number\",");
            promptBuilder.AppendLine("  \"invoiceDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("  \"dueDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("  \"totalAmount\": 123.45,");
            promptBuilder.AppendLine("  \"taxAmount\": 23.45,");
            promptBuilder.AppendLine("  \"currency\": \"EUR\"");
            promptBuilder.AppendLine("}");

            return promptBuilder.ToString();
        }

        private string BuildInvoicePartiesPrompt()
        {
            var promptBuilder = new System.Text.StringBuilder();

            promptBuilder.AppendLine("### INVOICE PARTIES EXTRACTION ###");
            promptBuilder.AppendLine("You are an expert in extracting structured data from invoice images.");
            promptBuilder.AppendLine("Your task is to analyze the provided invoice images and extract vendor and customer information.");

            promptBuilder.AppendLine("\n### EXTRACTION INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Extract the following information:");
            promptBuilder.AppendLine("   - Vendor name");
            promptBuilder.AppendLine("   - Vendor address"); // Added address
            promptBuilder.AppendLine("   - Vendor contact information");
            promptBuilder.AppendLine("   - Customer name"); // Added customer info
            promptBuilder.AppendLine("   - Customer address"); // Added customer info
           
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"vendorName\": \"Vendor name\",");
            promptBuilder.AppendLine("  \"vendorAddress\": \"Vendor address\",");
            promptBuilder.AppendLine("  \"vendorContact\": \"Vendor contact info\",");
            promptBuilder.AppendLine("  \"customerName\": \"Customer name\",");
            promptBuilder.AppendLine("  \"customerAddress\": \"Customer address\"");
            promptBuilder.AppendLine("}");

            return promptBuilder.ToString();
        }

        private string BuildInvoiceLineItemsPrompt()
        {
            var promptBuilder = new System.Text.StringBuilder();

            promptBuilder.AppendLine("### INVOICE LINE ITEMS EXTRACTION ###");
            promptBuilder.AppendLine("You are an expert in extracting structured data from invoice images.");
            promptBuilder.AppendLine("Your task is to analyze the provided invoice images and extract line items, payment details, and notes.");

            promptBuilder.AppendLine("\n### EXTRACTION INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Extract line items, including:");
            promptBuilder.AppendLine("   - Item description");
            promptBuilder.AppendLine("   - Quantity");
            promptBuilder.AppendLine("   - Unit price");
            promptBuilder.AppendLine("   - Total price per item");
            promptBuilder.AppendLine("2. Extract payment details:");
            promptBuilder.AppendLine("   - Payment terms");
            promptBuilder.AppendLine("   - Payment method");
            promptBuilder.AppendLine("3. Extract any additional notes or important information.");
            promptBuilder.AppendLine("4. Provide an overall confidence score (0.0-1.0) for the extracted line items and payment details.");


            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"lineItems\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"Item description\",");
            promptBuilder.AppendLine("      \"quantity\": 1,");
            promptBuilder.AppendLine("      \"unitPrice\": 100.00,");
            promptBuilder.AppendLine("      \"totalPrice\": 100.00");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"paymentTerms\": \"Payment terms\",");
            promptBuilder.AppendLine("  \"paymentMethod\": \"Payment method\",");
            promptBuilder.AppendLine("  \"notes\": \"Any additional important information\",");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0");
            promptBuilder.AppendLine("}");

            return promptBuilder.ToString();
        }
        
        private string BuildDocumentTypePrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### DOCUMENT TYPE VERIFICATION ###");
            promptBuilder.AppendLine("You are a document classification expert.");
            promptBuilder.AppendLine("Your task is to determine if the following text comes from a valid invoice document.");
            promptBuilder.AppendLine("If it's not an invoice, identify what type of document it appears to be.");
            
            // Add text content
            promptBuilder.AppendLine("\n### DOCUMENT TEXT ###");
            if (!string.IsNullOrEmpty(invoice.RawText))
            {
                // Provide more context if available
                promptBuilder.AppendLine($"Vendor: {invoice.VendorName ?? "N/A"}");
                promptBuilder.AppendLine($"Invoice Number: {invoice.InvoiceNumber ?? "N/A"}");
                promptBuilder.AppendLine($"Total Amount: {invoice.TotalAmount.ToString("C")}"); // Reverted: TotalAmount is not nullable
                promptBuilder.AppendLine("\n--- Start of Document Text ---");
                promptBuilder.AppendLine(invoice.RawText.Substring(0, Math.Min(1500, invoice.RawText.Length))); // Increased length slightly
                promptBuilder.AppendLine("--- End of Document Text ---");
            }
            else
            {
                promptBuilder.AppendLine("(No text content available)");
                
                // Add metadata if available to help classification
                if (!string.IsNullOrEmpty(invoice.FileName))
                {
                    promptBuilder.AppendLine($"\nFilename: {invoice.FileName}");
                }
                if (!string.IsNullOrEmpty(invoice.VendorName))
                {
                     promptBuilder.AppendLine($"Vendor Name: {invoice.VendorName}");
                }
                if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
                {
                    promptBuilder.AppendLine($"Document number: {invoice.InvoiceNumber}");
                }
                if (invoice.InvoiceDate.HasValue)
                {
                    promptBuilder.AppendLine($"Document date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
                }
                 // Reverted: No need to check for null on non-nullable decimal
                 promptBuilder.AppendLine($"Total Amount: {invoice.TotalAmount.ToString("C")}"); 
            }
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isInvoice\": true/false,");
            promptBuilder.AppendLine("  \"detectedDocumentType\": \"Invoice/Quote/Receipt/Contract/etc.\",");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"invoiceElements\": [\"List of invoice elements found\"],");
            promptBuilder.AppendLine("  \"missingElements\": [\"List of invoice elements missing\"],");
            promptBuilder.AppendLine("  \"explanation\": \"Explanation of document classification\"");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private void ParseInvoiceHeaderResponse(string responseText, Invoice invoice)
        {
             try
            {
                 // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini invoice header response");
                    return;
                }
                var jsonResponse = jsonMatch.Value;

                 // Use case-insensitive deserialization and allow trailing commas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Update invoice with extracted header data
                if (root.TryGetProperty("invoiceNumber", out var invoiceNumberElement) && invoiceNumberElement.ValueKind == JsonValueKind.String)
                    invoice.InvoiceNumber = invoiceNumberElement.GetString();
                
                if (root.TryGetProperty("invoiceDate", out var invoiceDateElement) && invoiceDateElement.ValueKind == JsonValueKind.String && DateTime.TryParse(invoiceDateElement.GetString(), out var dateValue))
                    invoice.InvoiceDate = dateValue;

                if (root.TryGetProperty("dueDate", out var dueDateElement) && dueDateElement.ValueKind == JsonValueKind.String && DateTime.TryParse(dueDateElement.GetString(), out dateValue))
                    invoice.DueDate = dateValue;

                if (root.TryGetProperty("totalAmount", out var totalAmountElement))
                {
                    if (totalAmountElement.ValueKind == JsonValueKind.Number)
                        invoice.TotalAmount = totalAmountElement.GetDecimal();
                    else if (totalAmountElement.ValueKind == JsonValueKind.String && decimal.TryParse(totalAmountElement.GetString(), out var amountValue))
                        invoice.TotalAmount = amountValue;
                }

                if (root.TryGetProperty("taxAmount", out var taxAmountElement))
                {
                    if (taxAmountElement.ValueKind == JsonValueKind.Number)
                    {
                        // Assuming TaxAmount exists on Invoice model
                        // invoice.TaxAmount = taxAmountElement.GetDecimal(); 
                    }
                    else if (taxAmountElement.ValueKind == JsonValueKind.String && decimal.TryParse(taxAmountElement.GetString(), out var taxValue))
                    {
                         // invoice.TaxAmount = taxValue;
                    }
                }

                if (root.TryGetProperty("currency", out var currencyElement) && currencyElement.ValueKind == JsonValueKind.String)
                {
                    // Assuming Currency exists on Invoice model
                    // invoice.Currency = currencyElement.GetString();
                }
            }
            catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Error parsing JSON from invoice header response: {ResponseText}", responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoice header extraction response");
            }
        }

        private void ParseInvoicePartiesResponse(string responseText, Invoice invoice)
        {
             try
            {
                 // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini invoice parties response");
                    return;
                }
                var jsonResponse = jsonMatch.Value;

                 // Use case-insensitive deserialization and allow trailing commas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Update invoice with extracted parties data
                if (root.TryGetProperty("vendorName", out var vendorNameElement) && vendorNameElement.ValueKind == JsonValueKind.String)
                    invoice.VendorName = vendorNameElement.GetString();
                
                if (root.TryGetProperty("vendorAddress", out var vendorAddressElement) && vendorAddressElement.ValueKind == JsonValueKind.String)
                {
                     // Assuming VendorAddress exists on Invoice model
                    // invoice.VendorAddress = vendorAddressElement.GetString();
                }
                
                if (root.TryGetProperty("vendorContact", out var vendorContactElement) && vendorContactElement.ValueKind == JsonValueKind.String)
                {
                     // Assuming VendorContact exists on Invoice model
                    // invoice.VendorContact = vendorContactElement.GetString();
                }

                if (root.TryGetProperty("customerName", out var customerNameElement) && customerNameElement.ValueKind == JsonValueKind.String)
                {
                     // Assuming CustomerName exists on Invoice model
                    // invoice.CustomerName = customerNameElement.GetString();
                }

                if (root.TryGetProperty("customerAddress", out var customerAddressElement) && customerAddressElement.ValueKind == JsonValueKind.String)
                {
                     // Assuming CustomerAddress exists on Invoice model
                    // invoice.CustomerAddress = customerAddressElement.GetString();
                }
            }
             catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Error parsing JSON from invoice parties response: {ResponseText}", responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoice parties extraction response");
            }
        }

         private void ParseInvoiceLineItemsResponse(string responseText, Invoice invoice)
        {
             try
            {
                 // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini invoice line items response");
                    return;
                }
                var jsonResponse = jsonMatch.Value;

                 // Use case-insensitive deserialization and allow trailing commas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Update invoice with extracted line items and payment data
                if (root.TryGetProperty("lineItems", out var lineItemsElement) && lineItemsElement.ValueKind == JsonValueKind.Array)
                {
                    invoice.LineItems = new List<InvoiceLineItem>(); // Initialize or clear existing items
                    foreach (var item in lineItemsElement.EnumerateArray())
                    {
                        var lineItem = new InvoiceLineItem();
                        if (item.TryGetProperty("description", out var descElement) && descElement.ValueKind == JsonValueKind.String)
                            lineItem.Description = descElement.GetString();
                        
                        if (item.TryGetProperty("quantity", out var qtyElement))
                        {
                            if (qtyElement.ValueKind == JsonValueKind.Number)
                                lineItem.Quantity = (int)qtyElement.GetDecimal();
                            else if (qtyElement.ValueKind == JsonValueKind.String && decimal.TryParse(qtyElement.GetString(), out var qtyValue))
                                lineItem.Quantity = (int)qtyValue;
                        }

                        if (item.TryGetProperty("unitPrice", out var unitPriceElement))
                        {
                             if (unitPriceElement.ValueKind == JsonValueKind.Number)
                                lineItem.UnitPrice = unitPriceElement.GetDecimal();
                            else if (unitPriceElement.ValueKind == JsonValueKind.String && decimal.TryParse(unitPriceElement.GetString(), out var priceValue))
                                lineItem.UnitPrice = priceValue;
                        }

                         if (item.TryGetProperty("totalPrice", out var totalPriceElement))
                        {
                              if (totalPriceElement.ValueKind == JsonValueKind.Number)
                                 lineItem.TotalPrice = totalPriceElement.GetDecimal();
                             else if (totalPriceElement.ValueKind == JsonValueKind.String && decimal.TryParse(totalPriceElement.GetString(), out var priceValue)) // Declare priceValue here
                                 lineItem.TotalPrice = priceValue;
                         }
                         invoice.LineItems.Add(lineItem);
                    }
                }

                 if (root.TryGetProperty("paymentTerms", out var paymentTermsElement) && paymentTermsElement.ValueKind == JsonValueKind.String)
                 {
                     // Assuming PaymentTerms exists on Invoice model
                    // invoice.PaymentTerms = paymentTermsElement.GetString();
                 }

                 if (root.TryGetProperty("paymentMethod", out var paymentMethodElement) && paymentMethodElement.ValueKind == JsonValueKind.String)
                 {
                      // Assuming PaymentMethod exists on Invoice model
                    // invoice.PaymentMethod = paymentMethodElement.GetString();
                 }

                 if (root.TryGetProperty("notes", out var notesElement) && notesElement.ValueKind == JsonValueKind.String)
                 {
                      // Assuming Notes exists on Invoice model
                    // invoice.Notes = notesElement.GetString();
                     if (string.IsNullOrEmpty(invoice.RawText)) // Populate RawText if empty
                        invoice.RawText = notesElement.GetString();
                 }

                 if (root.TryGetProperty("confidence", out var confidenceElement) && confidenceElement.ValueKind == JsonValueKind.Number)
                 {
                     // Assuming ExtractionConfidence exists on Invoice model
                    // invoice.ExtractionConfidence = confidenceElement.GetDouble();
                 }
            }
             catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Error parsing JSON from invoice line items response: {ResponseText}", responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoice line items extraction response");
            }
        }


        private class DocumentVerificationResult
        {
            public bool IsInvoice { get; set; }
            public string DetectedDocumentType { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public List<string> InvoiceElements { get; set; } = new List<string>();
            public List<string> MissingElements { get; set; } = new List<string>();
            public string Explanation { get; set; } = string.Empty;
        }

        private DocumentVerificationResult ParseDocumentTypeResponse(string responseText, ValidationResult result)
        {
            var verification = new DocumentVerificationResult();
            
            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini document verification response");
                    verification.IsInvoice = false;
                    verification.DetectedDocumentType = "Unknown";
                    verification.Explanation = "Failed to parse AI response";
                    return verification;
                }
                
                var jsonResponse = jsonMatch.Value;
                
                // Use case-insensitive deserialization and allow trailing commas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                
                // Parse using JsonDocument
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;
                
                // Parse the verification result
                if (root.TryGetProperty("isInvoice", out var isInvoiceElement) && 
                    isInvoiceElement.ValueKind == JsonValueKind.True)
                {
                    verification.IsInvoice = true;
                }
                
                if (root.TryGetProperty("detectedDocumentType", out var docTypeElement))
                {
                    if (docTypeElement.ValueKind == JsonValueKind.String)
                    {
                        verification.DetectedDocumentType = docTypeElement.GetString() ?? "Unknown";
                    }
                    else
                    {
                        _logger.LogWarning("detectedDocumentType property is not a string or is missing in Gemini document verification response");
                        verification.DetectedDocumentType = "Unknown";
                    }
                }
                else
                {
                    _logger.LogWarning("detectedDocumentType property is missing in Gemini document verification response");
                    verification.DetectedDocumentType = "Unknown";
                }
                
                if (root.TryGetProperty("confidence", out var confidenceElement))
                {
                    if (confidenceElement.ValueKind == JsonValueKind.Number)
                    {
                        verification.Confidence = confidenceElement.GetDouble();
                    }
                    else
                    {
                        _logger.LogWarning("confidence property is not a number or is missing in Gemini document verification response");
                        verification.Confidence = 0.0;
                    }
                }
                else
                {
                     _logger.LogWarning("confidence property is missing in Gemini document verification response");
                     verification.Confidence = 0.0;
                }
                if (root.TryGetProperty("invoiceElements", out var elementsElement) && 
                    elementsElement.ValueKind == JsonValueKind.Array)
                {
                    verification.InvoiceElements = new List<string>(); // Initialize list
                    foreach (var element in elementsElement.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            verification.InvoiceElements.Add(element.GetString() ?? string.Empty);
                        }
                    }
                    result.PresentInvoiceElements = verification.InvoiceElements;
                }
                
                if (root.TryGetProperty("missingElements", out var missingElement) && 
                    missingElement.ValueKind == JsonValueKind.Array)
                {
                     verification.MissingElements = new List<string>(); // Initialize list
                    foreach (var element in missingElement.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            verification.MissingElements.Add(element.GetString() ?? string.Empty);
                        }
                    }
                    result.MissingInvoiceElements = verification.MissingElements;
                }
                
                if (root.TryGetProperty("explanation", out var explanationElement) && 
                    explanationElement.ValueKind == JsonValueKind.String)
                {
                    verification.Explanation = explanationElement.GetString() ?? string.Empty;
                    
                    // Add explanation to result issues
                    result.AddIssue(ValidationSeverity.Info, verification.Explanation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing document verification response");
                verification.IsInvoice = false;
                verification.DetectedDocumentType = "Unknown";
                verification.Explanation = $"Error parsing AI response: {ex.Message}";
            }
            
            return verification;
        }

        #endregion
    }
}
