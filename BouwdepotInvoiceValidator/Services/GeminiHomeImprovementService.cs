using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services.Prompts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for validating home improvement invoices using Gemini API
    /// </summary>
    public class GeminiHomeImprovementService : GeminiServiceBase
    {
        private readonly ILogger<GeminiHomeImprovementService> _homeLogger;
        private readonly PromptTemplateService _promptService;
        
        public GeminiHomeImprovementService(
            ILogger<GeminiHomeImprovementService> logger, 
            IConfiguration configuration,
            PromptTemplateService promptService) 
            : base(logger, configuration)
        {
            _homeLogger = logger;
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        }
        
        /// <summary>
        /// Validates if an invoice is related to home improvement
        /// </summary>
        public async Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice)
        {
            _homeLogger.LogInformation("Validating if invoice is related to home improvement: {FileName}", invoice.FileName);
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Prepare the invoice data for Gemini
                var prompt = BuildHomeImprovementPrompt(invoice);
                
                // Call Gemini API with specific operation name for better logs
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "HomeImprovementValidation");
                result.RawGeminiResponse = response;
                
                // Parse the response
                var isHomeImprovement = ParseHomeImprovementResponse(response, result);
                result.IsHomeImprovement = isHomeImprovement;
                
                if (!isHomeImprovement)
                {
                    _homeLogger.LogWarning("Invoice not validated as home improvement expense: {FileName}", invoice.FileName);
                    
                    result.AddIssue(ValidationSeverity.Error, 
                        "The invoice does not appear to be related to home improvement expenses.");
                    result.IsValid = false;
                }
                else
                {
                    _homeLogger.LogInformation("Invoice validated as valid home improvement expense: {FileName}", invoice.FileName);
                    
                    result.AddIssue(ValidationSeverity.Info, 
                        "The invoice appears to be valid home improvement expenses.");
                    result.IsValid = true;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _homeLogger.LogError(ex, "Error validating home improvement with Gemini for {FileName}", invoice.FileName);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating with Gemini: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        /// <summary>
        /// Checks for signs of tampering or fraud in the invoice
        /// </summary>
        public async Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering)
        {
            _homeLogger.LogInformation("Checking for fraud indicators in invoice: {FileName}", invoice.FileName);
            
            try
            {
                // If we already detected PDF tampering, no need to check with Gemini
                if (detectedTampering)
                {
                    _homeLogger.LogWarning("PDF tampering already detected, skipping Gemini fraud check: {FileName}", invoice.FileName);
                    return true;
                }
                
                // Prepare the fraud detection prompt
                var prompt = BuildFraudDetectionPrompt(invoice);
                
                // Call Gemini API with specific operation name for better logs
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "FraudDetection");
                
                // Parse the response
                bool possibleFraud = ParseFraudDetectionResponse(response);
                
                if (possibleFraud)
                {
                    _homeLogger.LogWarning("Gemini detected possible fraud in the invoice: {FileName}", invoice.FileName);
                }
                else
                {
                    _homeLogger.LogInformation("No fraud indicators detected by Gemini: {FileName}", invoice.FileName);
                }
                
                return possibleFraud;
            }
            catch (Exception ex)
            {
                _homeLogger.LogError(ex, "Error checking for fraud with Gemini for {FileName}", invoice.FileName);
                // If we encounter an error, we should assume no fraud to avoid false positives
                return false;
            }
        }
        
        private string BuildHomeImprovementPrompt(Invoice invoice)
        {
            _homeLogger.LogDebug("Building home improvement validation prompt for {FileName}", invoice.FileName);
            
            try
            {
                // Try to get the prompt from the template service
                var parameters = new Dictionary<string, string>
                {
                    { "fileName", invoice.FileName ?? "Unknown" },
                    { "vendorName", invoice.VendorName ?? "Unknown" }
                };
                
                var prompt = _promptService.GetPrompt("MultiModalHomeImprovement", parameters);
                
                // If we got a valid prompt, return it
                if (!string.IsNullOrEmpty(prompt))
                {
                    _homeLogger.LogDebug("Successfully built home improvement prompt from template");
                    return prompt;
                }
            }
            catch (Exception ex)
            {
                _homeLogger.LogWarning(ex, "Error using prompt template for home improvement. Falling back to default prompt.");
            }
            
            // Fallback to the old hardcoded prompt if template fails
            _homeLogger.LogWarning("Using fallback hardcoded prompt for home improvement");
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### VISUAL INVOICE ANALYSIS: HOME IMPROVEMENT CLASSIFICATION ###");
            promptBuilder.AppendLine("You are a specialized visual validator for Dutch construction and home improvement expenses.");
            promptBuilder.AppendLine("Your task is to analyze ONLY THE VISUAL ELEMENTS of this invoice to determine if it's related to home improvement.");
            promptBuilder.AppendLine("DO NOT read or analyze the text content directly - focus exclusively on visual patterns, layout, and document structure.");
            
            promptBuilder.AppendLine("\n### CONTEXT ###");
            promptBuilder.AppendLine("- Home improvement invoices often have distinctive visual characteristics");
            promptBuilder.AppendLine("- Look for construction-related visual elements like diagrams, technical drawings, floor plans");
            promptBuilder.AppendLine("- Construction invoices typically have structured line item tables with quantities and measurements");
            promptBuilder.AppendLine("- Professional construction companies often use specific layout formats recognizable visually");
            
            promptBuilder.AppendLine("\n### VISUAL ANALYSIS INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Analyze the overall document structure and professionalism");
            promptBuilder.AppendLine("2. Identify visual patterns common in construction invoices vs. retail receipts");
            promptBuilder.AppendLine("3. Look for construction-specific visual elements (diagrams, technical symbols)");
            promptBuilder.AppendLine("4. Assess the visual structure of any tables, especially those that appear to be line items");
            promptBuilder.AppendLine("5. Note any suspicious visual elements like inconsistent formatting or alignment");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"reasoning\": \"Your detailed VISUAL analysis explanation\",");
            promptBuilder.AppendLine("  \"likelyCategory\": \"The likely category based on visual patterns (e.g., Roofing, Plumbing, General Construction)\"");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        private string BuildFraudDetectionPrompt(Invoice invoice)
        {
            _homeLogger.LogDebug("Building fraud detection prompt for {FileName}", invoice.FileName);
            
            try
            {
                // Try to get the prompt from the template service
                var parameters = new Dictionary<string, string>
                {
                    { "fileName", invoice.FileName ?? "Unknown" },
                    { "vendorName", invoice.VendorName ?? "Unknown" }
                };
                
                var prompt = _promptService.GetPrompt("FraudDetection", parameters);
                
                // If we got a valid prompt, return it
                if (!string.IsNullOrEmpty(prompt))
                {
                    _homeLogger.LogDebug("Successfully built fraud detection prompt from template");
                    return prompt;
                }
            }
            catch (Exception ex)
            {
                _homeLogger.LogWarning(ex, "Error using prompt template for fraud detection. Falling back to default prompt.");
            }
            
            // Fallback to the old hardcoded prompt if template fails
            _homeLogger.LogWarning("Using fallback hardcoded prompt for fraud detection");
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### VISUAL FRAUD DETECTION ###");
            promptBuilder.AppendLine("You are an expert in detecting visual indications of potentially fraudulent invoices.");
            promptBuilder.AppendLine("Using ONLY VISUAL ANALYSIS, examine this invoice for signs of tampering, manipulation, or fraud.");
            promptBuilder.AppendLine("DO NOT read or analyze the text content directly - focus exclusively on visual patterns that may indicate fraud.");
            
            promptBuilder.AppendLine("\n### VISUAL FRAUD INDICATORS ###");
            promptBuilder.AppendLine("Focus on these key visual fraud indicators:");
            promptBuilder.AppendLine("1. INCONSISTENT FORMATTING: Different fonts, sizes, or styles within the document");
            promptBuilder.AppendLine("2. ALIGNMENT ISSUES: Irregular spacing, misaligned elements, or unusual layout");
            promptBuilder.AppendLine("3. DIGITAL MANIPULATION: Signs of digital editing, blurring, or artifacts");
            promptBuilder.AppendLine("4. IRREGULAR PATTERNS: Visual patterns that deviate from standard invoice formats");
            promptBuilder.AppendLine("5. STAMP/SIGNATURE ANALYSIS: Visual assessment of any stamps or signatures");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"possibleFraud\": true/false,");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"visualEvidence\": \"Description of specific visual evidence supporting your conclusion\",");
            promptBuilder.AppendLine("  \"visualIndicators\": [\"List of specific visual fraud indicators detected\"]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        private bool ParseHomeImprovementResponse(string responseText, ValidationResult result)
        {
            _homeLogger.LogDebug("Parsing home improvement validation response");
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<HomeImprovementResponse>(responseText);
                
                if (response != null)
                {
                    _homeLogger.LogInformation("Home improvement classification: IsHomeImprovement={IsHomeImprovement}, Confidence={Confidence:P0}", 
                        response.IsHomeImprovement, response.Confidence);
                    
                    if (!string.IsNullOrEmpty(response.LikelyCategory))
                    {
                        _homeLogger.LogInformation("Detected likely category: {Category}", response.LikelyCategory);
                    }
                    
                    // Add reasoning as an info issue
                    result.AddIssue(ValidationSeverity.Info, 
                        $"Gemini assessment: {response.Reasoning} (Confidence: {response.Confidence:P0})");
                    
                    return response.IsHomeImprovement;
                }
                else
                {
                    _homeLogger.LogWarning("Failed to parse home improvement response as JSON, performing keyword analysis");
                    
                    // If we can't parse JSON, check for keywords
                    var isHomeImprovement = responseText.Contains("home improvement", StringComparison.OrdinalIgnoreCase) && 
                           !responseText.Contains("not related to home improvement", StringComparison.OrdinalIgnoreCase);
                           
                    _homeLogger.LogInformation("Keyword analysis result for home improvement: {IsHomeImprovement}", isHomeImprovement);
                    
                    return isHomeImprovement;
                }
            }
            catch (Exception ex)
            {
                _homeLogger.LogError(ex, "Error parsing Gemini home improvement response: {ErrorMessage}", ex.Message);
                // Default to true to avoid false negatives
                return true;
            }
        }
        
        private bool ParseFraudDetectionResponse(string responseText)
        {
            _homeLogger.LogDebug("Parsing fraud detection response");
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<FraudResponse>(responseText);
                
                if (response != null)
                {
                    _homeLogger.LogInformation("Fraud detection result: PossibleFraud={PossibleFraud}, Confidence={Confidence:P0}", 
                        response.PossibleFraud, response.Confidence);
                    
                    _homeLogger.LogDebug("Fraud detection evidence: {Evidence}", response.VisualEvidence);
                    
                    // Log visual indicators if any were detected
                    if (response.VisualIndicators.Count > 0)
                    {
                        _homeLogger.LogInformation("Detected visual fraud indicators: {Indicators}", 
                            string.Join(", ", response.VisualIndicators));
                    }
                    
                    return response.PossibleFraud;
                }
                else
                {
                    _homeLogger.LogWarning("Failed to parse fraud detection response as JSON, performing keyword analysis");
                    
                    // If we can't parse JSON, check for keywords
                    var possibleFraud = responseText.Contains("suspicious", StringComparison.OrdinalIgnoreCase) || 
                           responseText.Contains("fraud", StringComparison.OrdinalIgnoreCase) ||
                           responseText.Contains("inconsistent", StringComparison.OrdinalIgnoreCase);
                           
                    _homeLogger.LogInformation("Keyword analysis result for fraud detection: {PossibleFraud}", possibleFraud);
                    
                    return possibleFraud;
                }
            }
            catch (Exception ex)
            {
                _homeLogger.LogError(ex, "Error parsing Gemini fraud response: {ErrorMessage}", ex.Message);
                // Default to false to avoid false positives
                return false;
            }
        }
        
        internal class HomeImprovementResponse
        {
            [JsonPropertyName("isHomeImprovement")]
            public bool IsHomeImprovement { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
            
            [JsonPropertyName("likelyCategory")]
            public string LikelyCategory { get; set; } = string.Empty;
        }
        
        internal class FraudResponse
        {
            [JsonPropertyName("possibleFraud")]
            public bool PossibleFraud { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("visualEvidence")]
            public string VisualEvidence { get; set; } = string.Empty;
            
            [JsonPropertyName("visualIndicators")]
            public List<string> VisualIndicators { get; set; } = new List<string>();
        }
    }
}
