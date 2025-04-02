using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Models.Analysis;

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for advanced analysis of invoices using Gemini AI
    /// </summary>
    public class GeminiAdvancedAnalysisService : GeminiServiceBase
    {
        public GeminiAdvancedAnalysisService(ILogger<GeminiAdvancedAnalysisService> logger, IConfiguration configuration)
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
                    _logger.LogWarning("Invoice data extraction requested but no page images are available");
                    return invoice;
                }
                
                // Prepare the extraction prompt
                var prompt = BuildExtractionPrompt();
                
                // Call Gemini API with multi-modal content
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "InvoiceDataExtraction");
                
                // Parse the response and update the invoice
                var updatedInvoice = ParseExtractionResponse(response, invoice);
                
                _logger.LogInformation("Invoice data extraction completed for {FileName}", invoice.FileName);
                
                return updatedInvoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice data from images");
                return invoice;
            }
        }

        #region Helper Methods
        
        private string BuildExtractionPrompt()
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### INVOICE DATA EXTRACTION ###");
            promptBuilder.AppendLine("You are a specialized invoice data extraction system.");
            promptBuilder.AppendLine("Your task is to analyze the provided invoice image and extract key information.");
            
            promptBuilder.AppendLine("\n### EXTRACTION INSTRUCTIONS ###");
            promptBuilder.AppendLine("Extract the following information from the invoice:");
            promptBuilder.AppendLine("1. Invoice number");
            promptBuilder.AppendLine("2. Invoice date");
            promptBuilder.AppendLine("3. Due date (if available)");
            promptBuilder.AppendLine("4. Vendor name");
            promptBuilder.AppendLine("5. Vendor address");
            promptBuilder.AppendLine("6. Vendor contact information (phone, email, website)");
            promptBuilder.AppendLine("7. Customer name and address");
            promptBuilder.AppendLine("8. Payment terms");
            promptBuilder.AppendLine("9. Line items (description, quantity, unit price, total)");
            promptBuilder.AppendLine("10. Subtotal");
            promptBuilder.AppendLine("11. Tax amount and rate");
            promptBuilder.AppendLine("12. Total amount");
            promptBuilder.AppendLine("13. Payment instructions (bank details, etc.)");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"invoiceNumber\": \"string\",");
            promptBuilder.AppendLine("  \"invoiceDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("  \"dueDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("  \"vendorName\": \"string\",");
            promptBuilder.AppendLine("  \"vendorAddress\": \"string\",");
            promptBuilder.AppendLine("  \"vendorContact\": {");
            promptBuilder.AppendLine("    \"phone\": \"string\",");
            promptBuilder.AppendLine("    \"email\": \"string\",");
            promptBuilder.AppendLine("    \"website\": \"string\"");
            promptBuilder.AppendLine("  },");
            promptBuilder.AppendLine("  \"customerInfo\": \"string\",");
            promptBuilder.AppendLine("  \"paymentTerms\": \"string\",");
            promptBuilder.AppendLine("  \"lineItems\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"string\",");
            promptBuilder.AppendLine("      \"quantity\": number,");
            promptBuilder.AppendLine("      \"unitPrice\": number,");
            promptBuilder.AppendLine("      \"totalPrice\": number");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"subtotal\": number,");
            promptBuilder.AppendLine("  \"taxAmount\": number,");
            promptBuilder.AppendLine("  \"taxRate\": number,");
            promptBuilder.AppendLine("  \"totalAmount\": number,");
            promptBuilder.AppendLine("  \"paymentInstructions\": \"string\",");
            promptBuilder.AppendLine("  \"currency\": \"string\"");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private Invoice ParseExtractionResponse(string responseText, Invoice invoice)
        {
            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini extraction response");
                    return invoice;
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
                
                // Update invoice with extracted data
                
                // Invoice number
                if (root.TryGetProperty("invoiceNumber", out var invoiceNumberElement) && 
                    invoiceNumberElement.ValueKind == JsonValueKind.String)
                {
                    invoice.InvoiceNumber = invoiceNumberElement.GetString();
                }
                
                // Invoice date
                if (root.TryGetProperty("invoiceDate", out var invoiceDateElement) && 
                    invoiceDateElement.ValueKind == JsonValueKind.String)
                {
                    var dateString = invoiceDateElement.GetString();
                    if (DateTime.TryParse(dateString, out var date))
                    {
                        invoice.InvoiceDate = date;
                    }
                }
                
                // Due date
                if (root.TryGetProperty("dueDate", out var dueDateElement) && 
                    dueDateElement.ValueKind == JsonValueKind.String)
                {
                    var dateString = dueDateElement.GetString();
                    if (DateTime.TryParse(dateString, out var date))
                    {
                        invoice.DueDate = date;
                    }
                }
                
                // Vendor name
                if (root.TryGetProperty("vendorName", out var vendorNameElement) && 
                    vendorNameElement.ValueKind == JsonValueKind.String)
                {
                    invoice.VendorName = vendorNameElement.GetString();
                }
                
                // Vendor address
                if (root.TryGetProperty("vendorAddress", out var vendorAddressElement) && 
                    vendorAddressElement.ValueKind == JsonValueKind.String)
                {
                    invoice.VendorAddress = vendorAddressElement.GetString();
                }
                
                // Customer info - store in a temporary variable since Invoice doesn't have CustomerInfo property
                string customerInfo = string.Empty;
                if (root.TryGetProperty("customerInfo", out var customerInfoElement) && 
                    customerInfoElement.ValueKind == JsonValueKind.String)
                {
                    customerInfo = customerInfoElement.GetString() ?? string.Empty;
                    // We could store this in a custom field or log it, but for now we'll just log it
                    _logger.LogInformation("Extracted customer info: {CustomerInfo}", customerInfo);
                }
                
                // Payment terms - store in PaymentDetails
                if (root.TryGetProperty("paymentTerms", out var paymentTermsElement) && 
                    paymentTermsElement.ValueKind == JsonValueKind.String)
                {
                    // Store in the PaymentDetails object
                    if (invoice.PaymentDetails == null)
                    {
                        invoice.PaymentDetails = new PaymentDetails();
                    }
                    
                    // Store as a payment reference in PaymentDetails
                    invoice.PaymentDetails.PaymentReference = paymentTermsElement.GetString() ?? string.Empty;
                }
                
                // Line items
                if (root.TryGetProperty("lineItems", out var lineItemsElement) && 
                    lineItemsElement.ValueKind == JsonValueKind.Array)
                {
                    invoice.LineItems = new List<InvoiceLineItem>();
                    
                    foreach (var item in lineItemsElement.EnumerateArray())
                    {
                        var lineItem = new InvoiceLineItem();
                        
                        if (item.TryGetProperty("description", out var descElement) && 
                            descElement.ValueKind == JsonValueKind.String)
                        {
                            lineItem.Description = descElement.GetString();
                        }
                        
                        if (item.TryGetProperty("quantity", out var quantityElement) && 
                            quantityElement.ValueKind == JsonValueKind.Number)
                        {
                    // Convert decimal to int for Quantity
                    lineItem.Quantity = (int)quantityElement.GetDecimal();
                        }
                        
                        if (item.TryGetProperty("unitPrice", out var unitPriceElement) && 
                            unitPriceElement.ValueKind == JsonValueKind.Number)
                        {
                            lineItem.UnitPrice = unitPriceElement.GetDecimal();
                        }
                        
                        if (item.TryGetProperty("totalPrice", out var totalPriceElement) && 
                            totalPriceElement.ValueKind == JsonValueKind.Number)
                        {
                            lineItem.TotalPrice = totalPriceElement.GetDecimal();
                        }
                        
                        invoice.LineItems.Add(lineItem);
                    }
                }
                
                // Subtotal - Invoice doesn't have this property, so we'll calculate it from line items
                if (root.TryGetProperty("subtotal", out var subtotalElement) && 
                    subtotalElement.ValueKind == JsonValueKind.Number)
                {
                    decimal subtotal = subtotalElement.GetDecimal();
                    _logger.LogInformation("Extracted subtotal: {Subtotal}", subtotal);
                    
                    // We could calculate this from line items if needed
                    // For now, we'll just log it
                }
                
                // Tax amount - Invoice has VatAmount
                if (root.TryGetProperty("taxAmount", out var taxAmountElement) && 
                    taxAmountElement.ValueKind == JsonValueKind.Number)
                {
                    invoice.VatAmount = taxAmountElement.GetDecimal();
                }
                
                // Tax rate - Store in line items
                if (root.TryGetProperty("taxRate", out var taxRateElement) && 
                    taxRateElement.ValueKind == JsonValueKind.Number)
                {
                    decimal taxRate = taxRateElement.GetDecimal();
                    
                    // Apply the tax rate to all line items
                    foreach (var lineItem in invoice.LineItems)
                    {
                        lineItem.VatRate = taxRate;
                    }
                }
                
                // Total amount
                if (root.TryGetProperty("totalAmount", out var totalAmountElement) && 
                    totalAmountElement.ValueKind == JsonValueKind.Number)
                {
                    invoice.TotalAmount = totalAmountElement.GetDecimal();
                }
                
                // Payment instructions - Store in PaymentDetails
                if (root.TryGetProperty("paymentInstructions", out var paymentInstructionsElement) && 
                    paymentInstructionsElement.ValueKind == JsonValueKind.String)
                {
                    if (invoice.PaymentDetails == null)
                    {
                        invoice.PaymentDetails = new PaymentDetails();
                    }
                    
                    string instructions = paymentInstructionsElement.GetString() ?? string.Empty;
                    
                    // Extract IBAN if present
                    var ibanMatch = Regex.Match(instructions, @"[A-Z]{2}[0-9]{2}[A-Z0-9]{4}[0-9]{7}([A-Z0-9]?){0,16}");
                    if (ibanMatch.Success)
                    {
                        invoice.PaymentDetails.IBAN = ibanMatch.Value;
                    }
                    
                    // Store the full instructions in the PaymentReference field
                    if (!string.IsNullOrEmpty(invoice.PaymentDetails.PaymentReference))
                    {
                        invoice.PaymentDetails.PaymentReference += Environment.NewLine;
                    }
                    
                    invoice.PaymentDetails.PaymentReference += instructions;
                }
                
                // Currency
                if (root.TryGetProperty("currency", out var currencyElement) && 
                    currencyElement.ValueKind == JsonValueKind.String)
                {
                    invoice.Currency = currencyElement.GetString();
                }
                
                // Store the raw response in RawText for debugging
                if (string.IsNullOrEmpty(invoice.RawText))
                {
                    invoice.RawText = responseText;
                }
                else
                {
                    // Append to existing raw text
                    invoice.RawText += Environment.NewLine + "--- EXTRACTION RESPONSE ---" + Environment.NewLine + responseText;
                }
                
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing extraction response");
                return invoice;
            }
        }
        
        #endregion
    }
}
