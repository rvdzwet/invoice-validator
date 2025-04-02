using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Base class for AI model providers with shared functionality
    /// </summary>
    public abstract class AIModelProviderBase : IAIModelProvider
    {
        protected readonly ILogger _logger;
        
        protected AIModelProviderBase(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc />
        public abstract ModelProviderInfo GetProviderInfo();
        
        /// <inheritdoc />
        public abstract Task<InvoiceValidationResponse> ValidateInvoiceAsync(Invoice invoice, ValidationOptions options = null);
        
        /// <inheritdoc />
        public abstract Task<Invoice> ExtractDataFromInvoiceAsync(Invoice invoice);
        
        /// <summary>
        /// Builds a unified prompt for invoice validation
        /// </summary>
        /// <param name="invoice">The invoice to validate</param>
        /// <param name="options">Validation options</param>
        /// <returns>Formatted prompt string</returns>
        protected string BuildUnifiedValidationPrompt(Invoice invoice, ValidationOptions options)
        {
            options ??= new ValidationOptions();
            var promptBuilder = new StringBuilder();
            
            // Header with context
            promptBuilder.AppendLine("### BOUWDEPOT INVOICE VALIDATION ###");
            promptBuilder.AppendLine($"Analysis Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            promptBuilder.AppendLine("Framework: Dutch Construction Invoice Assessment 2025");
            promptBuilder.AppendLine("Regulatory Context: Dutch Mortgage Credit Directive, BouwDepot Regulations");
            
            // Task description
            promptBuilder.AppendLine("\n### TASK ###");
            promptBuilder.AppendLine("You are a specialized validator for Dutch construction and home improvement expenses.");
            promptBuilder.AppendLine("Analyze this invoice to determine:");
            promptBuilder.AppendLine("1. If it's a valid invoice document");
            promptBuilder.AppendLine("2. If it contains home improvement expenses eligible for Bouwdepot funding");
            promptBuilder.AppendLine("3. Extract key invoice data for processing");
            promptBuilder.AppendLine("4. Categorize and validate each line item");
            if (options.DetectFraud)
            {
                promptBuilder.AppendLine("5. Check for signs of fraud or tampering");
            }
            
            // Add language context
            promptBuilder.AppendLine($"\n### LANGUAGE CONTEXT: {options.LanguageCode} ###");
            promptBuilder.AppendLine("The invoice is most likely in Dutch. Common terms:");
            promptBuilder.AppendLine("- 'Factuur'/'Nota' = Invoice");
            promptBuilder.AppendLine("- 'Factuurdatum' = Invoice date");
            promptBuilder.AppendLine("- 'Factuurnummer' = Invoice number");
            promptBuilder.AppendLine("- 'Totaalbedrag'/'Totaal' = Total amount");
            promptBuilder.AppendLine("- 'BTW'/'BTW-nummer' = VAT/VAT Number");
            promptBuilder.AppendLine("- 'KvK'/'KvK-nummer' = Chamber of Commerce number");
            
            // Document validation criteria
            promptBuilder.AppendLine("\n### DOCUMENT VALIDATION CRITERIA ###");
            promptBuilder.AppendLine("A valid invoice should contain:");
            promptBuilder.AppendLine("1. Invoice number");
            promptBuilder.AppendLine("2. Invoice date");
            promptBuilder.AppendLine("3. Vendor details (name, address, contact information)");
            promptBuilder.AppendLine("4. Customer information");
            promptBuilder.AppendLine("5. Line items with descriptions, quantities and prices");
            promptBuilder.AppendLine("6. Total amount");
            promptBuilder.AppendLine("7. Payment information");
            promptBuilder.AppendLine("8. Tax information (VAT numbers, calculations)");
            
            // Home improvement criteria
            promptBuilder.AppendLine("\n### HOME IMPROVEMENT CRITERIA ###");
            promptBuilder.AppendLine("To qualify as home improvement for Bouwdepot funding, expenses must:");
            promptBuilder.AppendLine("1. PERMANENT ATTACHMENT: Items must be permanently attached to the property");
            promptBuilder.AppendLine("2. QUALITY IMPROVEMENT: Items must improve the quality or functionality of the home");
            promptBuilder.AppendLine("3. PROPERTY VALUE: Modifications should generally increase property value");
            promptBuilder.AppendLine("4. PROFESSIONAL INSTALLATION: Labor for professional installation is eligible");
            promptBuilder.AppendLine("5. REGULATORY COMPLIANCE: Modifications must comply with Dutch building regulations");
            
            // Bouwdepot eligibility examples
            promptBuilder.AppendLine("\n### ELIGIBLE EXPENSE CATEGORIES ###");
            promptBuilder.AppendLine("- Structural: Foundation work, load-bearing walls, roof construction");
            promptBuilder.AppendLine("- Exterior: Siding, roofing, windows, doors, insulation");
            promptBuilder.AppendLine("- Interior: Flooring, wall finishing, ceiling work, built-in cabinetry");
            promptBuilder.AppendLine("- Systems: Plumbing, electrical, heating, cooling, ventilation");
            promptBuilder.AppendLine("- Kitchen: Built-in appliances, countertops, fixed cabinetry");
            promptBuilder.AppendLine("- Bathroom: Fixtures, tiling, shower units, permanent installations");
            promptBuilder.AppendLine("- Energy Efficiency: Solar panels, heat pumps, insulation, energy-efficient systems");
            promptBuilder.AppendLine("- Professional Services: Architecture, engineering, installation labor");
            
            // Non-eligible examples
            promptBuilder.AppendLine("\n### NON-ELIGIBLE EXPENSES ###");
            promptBuilder.AppendLine("- Movable furniture and appliances not permanently installed");
            promptBuilder.AppendLine("- Decorative items (curtains, rugs, lamps)");
            promptBuilder.AppendLine("- Consumer electronics and entertainment systems");
            promptBuilder.AppendLine("- Garden plants and non-permanent landscaping");
            promptBuilder.AppendLine("- Regular maintenance (cleaning, minor repairs)");
            promptBuilder.AppendLine("- Tools and equipment (unless part of a permanent installation)");
            
            // Fraud detection criteria if enabled
            if (options.DetectFraud)
            {
                promptBuilder.AppendLine("\n### FRAUD DETECTION CRITERIA ###");
                promptBuilder.AppendLine("Look for these potential indicators of fraud:");
                promptBuilder.AppendLine("1. Inconsistent formatting within the document");
                promptBuilder.AppendLine("2. Missing or altered key invoice elements");
                promptBuilder.AppendLine("3. Unusual pricing or quantities");
                promptBuilder.AppendLine("4. Vague or non-specific item descriptions");
                promptBuilder.AppendLine("5. Irregularities in tax calculations");
                promptBuilder.AppendLine("6. Handwritten modifications or corrections");
                promptBuilder.AppendLine("7. Mismatched or suspicious dates");
            }
            
            // Invoice context
            promptBuilder.AppendLine("\n### INVOICE CONTEXT ###");
            if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                promptBuilder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
            }
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Invoice Date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
            }
            if (!string.IsNullOrEmpty(invoice.VendorName))
            {
                promptBuilder.AppendLine($"Vendor: {invoice.VendorName}");
            }
            if (!string.IsNullOrEmpty(invoice.VendorAddress))
            {
                promptBuilder.AppendLine($"Vendor Address: {invoice.VendorAddress}");
            }
            if (invoice.TotalAmount > 0)
            {
                promptBuilder.AppendLine($"Total Amount: {invoice.TotalAmount:C}");
            }
            
            // Line items to analyze
            if (invoice.LineItems != null && invoice.LineItems.Count > 0)
            {
                promptBuilder.AppendLine("\n### LINE ITEMS TO ANALYZE ###");
                var itemCount = Math.Min(options.MaxLineItems, invoice.LineItems.Count);
                for (int i = 0; i < itemCount; i++)
                {
                    var item = invoice.LineItems[i];
                    promptBuilder.AppendLine($"{i+1}. {item.Description}");
                    promptBuilder.AppendLine($"   Quantity: {item.Quantity}, Unit Price: {item.UnitPrice:C}, Total: {item.TotalPrice:C}");
                }
                
                if (invoice.LineItems.Count > options.MaxLineItems)
                {
                    promptBuilder.AppendLine($"(+ {invoice.LineItems.Count - options.MaxLineItems} more items not shown)");
                }
            }
            
            // Raw text if available
            if (!string.IsNullOrEmpty(invoice.RawText))
            {
                promptBuilder.AppendLine("\n### INVOICE RAW TEXT ###");
                // Include first 2000 characters of raw text
                promptBuilder.AppendLine(invoice.RawText.Substring(0, Math.Min(2000, invoice.RawText.Length)));
                if (invoice.RawText.Length > 2000)
                {
                    promptBuilder.AppendLine("(Text truncated due to length)");
                }
            }
            
            // Add additional context if provided
            if (!string.IsNullOrEmpty(options.AdditionalContext))
            {
                promptBuilder.AppendLine("\n### ADDITIONAL CONTEXT ###");
                promptBuilder.AppendLine(options.AdditionalContext);
            }
            
            // Output format instructions
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isValidInvoice\": boolean,");
            promptBuilder.AppendLine("  \"isHomeImprovement\": boolean,");
            promptBuilder.AppendLine("  \"confidenceScore\": 0-100,");
            promptBuilder.AppendLine("  \"extractedData\": {");
            promptBuilder.AppendLine("    \"invoiceNumber\": \"string\",");
            promptBuilder.AppendLine("    \"invoiceDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("    \"totalAmount\": number,");
            promptBuilder.AppendLine("    \"vendorName\": \"string\",");
            promptBuilder.AppendLine("    \"vendorAddress\": \"string\",");
            promptBuilder.AppendLine("    \"vendorKvkNumber\": \"string\",");
            promptBuilder.AppendLine("    \"vendorBtwNumber\": \"string\"");
            promptBuilder.AppendLine("  },");
            promptBuilder.AppendLine("  \"lineItems\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"string\",");
            promptBuilder.AppendLine("      \"interpretedAs\": \"string\",");
            promptBuilder.AppendLine("      \"category\": \"string\",");
            promptBuilder.AppendLine("      \"isEligible\": boolean,");
            promptBuilder.AppendLine("      \"isPermanentlyAttached\": boolean,");
            promptBuilder.AppendLine("      \"improvesHomeQuality\": boolean,");
            promptBuilder.AppendLine("      \"confidence\": 0-100,");
            promptBuilder.AppendLine("      \"notes\": \"string\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"categories\": [\"string\"],");
            promptBuilder.AppendLine("  \"primaryPurpose\": \"string\",");
            promptBuilder.AppendLine("  \"summary\": \"string\",");
            promptBuilder.AppendLine("  \"validationNotes\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"severity\": \"Info/Warning/Error\",");
            promptBuilder.AppendLine("      \"message\": \"string\",");
            promptBuilder.AppendLine("      \"section\": \"string\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            
            if (options.DetectFraud)
            {
                promptBuilder.AppendLine("  \"fraudIndicators\": [");
                promptBuilder.AppendLine("    {");
                promptBuilder.AppendLine("      \"indicator\": \"string\",");
                promptBuilder.AppendLine("      \"description\": \"string\",");
                promptBuilder.AppendLine("      \"evidence\": \"string\",");
                promptBuilder.AppendLine("      \"severity\": 0.0-1.0");
                promptBuilder.AppendLine("    }");
                promptBuilder.AppendLine("  ],");
            }
            
            if (options.IncludeAuditJustification)
            {
                promptBuilder.AppendLine("  \"auditJustification\": \"Detailed explanation of the validation decision for audit purposes\",");
                promptBuilder.AppendLine("  \"criteriaAssessments\": [");
                promptBuilder.AppendLine("    {");
                promptBuilder.AppendLine("      \"criterionName\": \"string\",");
                promptBuilder.AppendLine("      \"score\": 0-100,");
                promptBuilder.AppendLine("      \"evidence\": \"string\",");
                promptBuilder.AppendLine("      \"reasoning\": \"string\"");
                promptBuilder.AppendLine("    }");
                promptBuilder.AppendLine("  ],");
            }
            
            promptBuilder.AppendLine("  \"bouwdepotValidation\": {");
            promptBuilder.AppendLine("    \"qualityImprovementRule\": boolean,");
            promptBuilder.AppendLine("    \"permanentAttachmentRule\": boolean,");
            promptBuilder.AppendLine("    \"generalValidationNotes\": \"string\"");
            promptBuilder.AppendLine("  }");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        /// <summary>
        /// Extracts JSON from a response text that may contain additional text
        /// </summary>
        /// <param name="responseText">Raw response text</param>
        /// <returns>Extracted JSON string or the original text if no JSON is found</returns>
        protected string ExtractJsonFromResponse(string responseText)
        {
            try
            {
                // Try to find JSON inside the response using regex pattern for opening and closing braces
                var jsonMatch = Regex.Match(responseText, @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}");
                if (jsonMatch.Success)
                {
                    return jsonMatch.Value;
                }
                
                // Fallback: if no JSON found, return the original text
                return responseText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting JSON from response");
                return responseText;
            }
        }
        
        /// <summary>
        /// Sanitizes a JSON string for parsing
        /// </summary>
        /// <param name="json">Input JSON string</param>
        /// <returns>Sanitized JSON string</returns>
        protected string SanitizeJsonString(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;
                
            // Replace escaped characters
            string sanitized = json
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\'", "'")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
                
            // Remove any markdown code block markers
            sanitized = Regex.Replace(sanitized, @"```(?:json)?|```", string.Empty, RegexOptions.IgnoreCase);
            
            return sanitized.Trim();
        }
        
        /// <summary>
        /// Creates validation notes from model response
        /// </summary>
        /// <param name="messages">List of validation messages</param>
        /// <returns>List of structured validation notes</returns>
        protected List<ValidationNote> CreateValidationNotes(List<string> messages)
        {
            var notes = new List<ValidationNote>();
            
            foreach (var message in messages)
            {
                var severity = ValidationSeverity.Info;
                
                // Determine severity based on keywords
                if (message.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("fail", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("not eligible", StringComparison.OrdinalIgnoreCase))
                {
                    severity = ValidationSeverity.Error;
                }
                else if (message.Contains("warning", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("caution", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("concern", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("missing", StringComparison.OrdinalIgnoreCase))
                {
                    severity = ValidationSeverity.Warning;
                }
                
                notes.Add(new ValidationNote
                {
                    Severity = severity,
                    Message = message,
                    Section = DetermineSectionFromMessage(message)
                });
            }
            
            return notes;
        }
        
        /// <summary>
        /// Attempts to determine which section of validation a message relates to
        /// </summary>
        /// <param name="message">The validation message</param>
        /// <returns>Section identifier</returns>
        private string DetermineSectionFromMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return "General";
                
            message = message.ToLowerInvariant();
            
            if (message.Contains("invoice") && (message.Contains("number") || message.Contains("date")))
                return "Invoice Details";
                
            if (message.Contains("vendor") || message.Contains("supplier"))
                return "Vendor Information";
                
            if (message.Contains("line item") || message.Contains("product") || message.Contains("service"))
                return "Line Items";
                
            if (message.Contains("total") || message.Contains("amount") || message.Contains("price"))
                return "Amounts";
                
            if (message.Contains("attachment") || message.Contains("quality") || message.Contains("improvement"))
                return "Bouwdepot Criteria";
                
            if (message.Contains("fraud") || message.Contains("tamper") || message.Contains("suspicious"))
                return "Fraud Detection";
                
            return "General";
        }
    }
}
