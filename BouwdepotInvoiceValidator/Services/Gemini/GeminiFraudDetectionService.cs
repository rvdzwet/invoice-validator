using System.Text.Json;
using System.Text.RegularExpressions;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Models.Enhanced; // Using statement already exists

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for detecting potential fraud in invoices using Gemini AI
    /// </summary>
    public class GeminiFraudDetectionService : GeminiServiceBase
    {
        public GeminiFraudDetectionService(ILogger<GeminiFraudDetectionService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }

        /// <summary>
        /// Uses Gemini AI to check for signs of tampering or fraud in the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <param name="detectedTampering">Initial tampering detection result from PDF analysis</param>
        /// <returns>True if Gemini detects possible fraud, otherwise false</returns>
        public async Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering)
        {
            _logger.LogInformation("Checking for fraud indicators in invoice");
            
            try
            {
                // If we already detected PDF tampering, no need to check with Gemini
                if (detectedTampering)
                {
                    _logger.LogWarning("PDF tampering already detected, skipping Gemini fraud check");
                    return true;
                }
                
                // Prepare the fraud detection prompt
                var prompt = BuildFraudDetectionPrompt(invoice);
                
                // Call Gemini API with text-only prompt
                var response = await CallGeminiApiAsync(prompt, null, "FraudDetection");
                
                // Parse the response
                bool possibleFraud = ParseFraudDetectionResponse(response);
                
                if (possibleFraud)
                {
                    _logger.LogWarning("Gemini detected possible fraud in the invoice");
                }
                
                return possibleFraud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for fraud with Gemini");
                // If we encounter an error, we should assume no fraud to avoid false positives
                return false;
            }
        }

        /// <summary>
        /// Populates the fraud detection section of a validation result with detailed analysis
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <param name="result">The validation result to populate with fraud detection data</param>
        /// <returns>True if fraud was detected, false otherwise</returns>
        public async Task<bool> EnhancedFraudDetectionAsync(Invoice invoice, ValidationResult result)
        {
            _logger.LogInformation("Performing enhanced fraud detection analysis");
            
            try
            {
                // Prepare the enhanced fraud detection prompt
                var prompt = BuildEnhancedFraudDetectionPrompt(invoice);
                
                // Call Gemini API - use images if available for visual fraud detection
                var useImages = invoice.PageImages != null && invoice.PageImages.Count > 0;
                var response = await CallGeminiApiAsync(
                    prompt, 
                    useImages ? invoice.PageImages : null, 
                    "EnhancedFraudDetection"
                );
                
                // Parse the response and populate the validation result
                var possibleFraud = ParseEnhancedFraudDetectionResponse(response, result);
                result.PossibleTampering = possibleFraud;
                
                if (possibleFraud)
                {
                    _logger.LogWarning("Enhanced fraud detection found possible fraud indicators");
                    
                    // Add an issue to the validation result
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Possible fraud detected. Review the fraud detection details for more information.");
                }
                else
                {
                    // Add an info message if no fraud detected
                    result.AddIssue(ValidationSeverity.Info, 
                        "No significant fraud indicators detected in this invoice.");
                }
                
                return possibleFraud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in enhanced fraud detection");
                
                // Add an error to the validation result
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error performing fraud detection analysis: {ex.Message}");
                
                // Initialize fraud detection with default values
                result.FraudDetection = new FraudDetection
                {
                    RiskLevel = FraudRiskLevel.Low,
                    FraudRiskScore = 0,
                    RecommendedAction = $"Error during analysis: {ex.Message}"
                };
                
                return false;
            }
        }

        #region Helper Methods

        private string BuildFraudDetectionPrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### FRAUD DETECTION ANALYSIS ###");
            promptBuilder.AppendLine("You are a financial fraud detection expert specializing in invoice analysis.");
            promptBuilder.AppendLine("Your task is to analyze this invoice for potential signs of fraud or irregularities.");
            
            // Add invoice metadata
            promptBuilder.AppendLine("\n### INVOICE DETAILS ###");
            promptBuilder.AppendLine($"Filename: {invoice.FileName}");
            
            if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                promptBuilder.AppendLine($"Invoice number: {invoice.InvoiceNumber}");
            }
            
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Invoice date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
            }
            
            if (invoice.TotalAmount > 0)
            {
                promptBuilder.AppendLine($"Total amount: {invoice.TotalAmount:C}");
            }
            
            if (!string.IsNullOrEmpty(invoice.VendorName))
            {
                promptBuilder.AppendLine($"Vendor name: {invoice.VendorName}");
            }
            
            // Add line items if available
            if (invoice.LineItems != null && invoice.LineItems.Count > 0)
            {
                promptBuilder.AppendLine("\n### LINE ITEMS ###");
                foreach (var item in invoice.LineItems)
                {
                    promptBuilder.AppendLine($"- {item.Quantity}x {item.Description}: {item.TotalPrice:C}");
                }
            }
            else if (!string.IsNullOrEmpty(invoice.RawText))
            {
                promptBuilder.AppendLine("\n### EXTRACTED TEXT ###");
                promptBuilder.AppendLine(invoice.RawText.Substring(0, Math.Min(1000, invoice.RawText.Length)));
            }
            
            promptBuilder.AppendLine("\n### FRAUD CHECK INSTRUCTIONS ###");
            promptBuilder.AppendLine("- Look for unusual patterns, prices, quantities, or descriptions");
            promptBuilder.AppendLine("- Check for vague or generic descriptions without specific details");
            promptBuilder.AppendLine("- Examine for inconsistencies in calculations");
            promptBuilder.AppendLine("- Assess for missing standard invoice elements");
            promptBuilder.AppendLine("- Consider if pricing is abnormal for the described goods/services");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"possibleFraud\": true/false,");
            promptBuilder.AppendLine("  \"riskLevel\": \"Low/Medium/High\",");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"fraudIndicators\": [\"List of potential fraud indicators found\"],");
            promptBuilder.AppendLine("  \"explanation\": \"Detailed explanation of your fraud assessment\",");
            promptBuilder.AppendLine("  \"recommendedActions\": [\"Recommended follow-up actions if suspicious\"]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private string BuildEnhancedFraudDetectionPrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### ENHANCED FRAUD DETECTION ANALYSIS ###");
            promptBuilder.AppendLine("You are a financial fraud detection expert specializing in invoice analysis.");
            promptBuilder.AppendLine("Your task is to perform a comprehensive fraud analysis of this invoice, looking for any signs of tampering, fraud, or irregularities.");
            
            // Add invoice metadata
            promptBuilder.AppendLine("\n### INVOICE DETAILS ###");
            promptBuilder.AppendLine($"Filename: {invoice.FileName}");
            
            if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                promptBuilder.AppendLine($"Invoice number: {invoice.InvoiceNumber}");
            }
            
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Invoice date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
            }
            
            if (invoice.TotalAmount > 0)
            {
                promptBuilder.AppendLine($"Total amount: {invoice.TotalAmount:C}");
            }
            
            if (!string.IsNullOrEmpty(invoice.VendorName))
            {
                promptBuilder.AppendLine($"Vendor name: {invoice.VendorName}");
            }
            
            // Add line items if available
            if (invoice.LineItems != null && invoice.LineItems.Count > 0)
            {
                promptBuilder.AppendLine("\n### LINE ITEMS ###");
                foreach (var item in invoice.LineItems)
                {
                    promptBuilder.AppendLine($"- {item.Quantity}x {item.Description}: {item.TotalPrice:C}");
                }
            }
            else if (!string.IsNullOrEmpty(invoice.RawText))
            {
                promptBuilder.AppendLine("\n### EXTRACTED TEXT ###");
                promptBuilder.AppendLine(invoice.RawText.Substring(0, Math.Min(1000, invoice.RawText.Length)));
            }
            
            promptBuilder.AppendLine("\n### FRAUD CHECK INSTRUCTIONS ###");
            promptBuilder.AppendLine("Perform a thorough analysis, considering the following aspects:");
            promptBuilder.AppendLine("1. VISUAL INCONSISTENCIES (if images are provided):");
            promptBuilder.AppendLine("   - Signs of digital manipulation or editing");
            promptBuilder.AppendLine("   - Inconsistent fonts, formatting, or alignment");
            promptBuilder.AppendLine("   - Suspicious or blurry areas in the document");
            promptBuilder.AppendLine("2. CONTENT ANALYSIS:");
            promptBuilder.AppendLine("   - Unusual or vague descriptions");
            promptBuilder.AppendLine("   - Pricing inconsistencies or abnormalities");
            promptBuilder.AppendLine("   - Missing or suspicious invoice elements");
            promptBuilder.AppendLine("   - Calculation errors or discrepancies");
            promptBuilder.AppendLine("3. CONTEXTUAL EVALUATION:");
            promptBuilder.AppendLine("   - Vendor legitimacy concerns");
            promptBuilder.AppendLine("   - Date or timeline inconsistencies");
            promptBuilder.AppendLine("   - Unusual payment terms or conditions");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"fraudRiskScore\": 0-100,");
            promptBuilder.AppendLine("  \"riskLevel\": \"Low/Medium/High\",");
            promptBuilder.AppendLine("  \"possibleFraud\": true/false,");
            promptBuilder.AppendLine("  \"recommendedAction\": \"Action to take\",");
            promptBuilder.AppendLine("  \"requiresManualReview\": true/false,");
            promptBuilder.AppendLine("  \"summary\": \"Overall assessment of fraud risk\",");
            promptBuilder.AppendLine("  \"detectedIndicators\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"indicatorName\": \"Name of fraud indicator\",");
            promptBuilder.AppendLine("      \"description\": \"Description of indicator\",");
            promptBuilder.AppendLine("      \"severity\": 0.0-1.0,");
            promptBuilder.AppendLine("      \"category\": \"Visual/Content/Contextual\",");
            promptBuilder.AppendLine("      \"evidence\": \"Evidence supporting this indicator\",");
            promptBuilder.AppendLine("      \"confidence\": 0.0-1.0");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"suggestedVerificationSteps\": [\"Steps to verify authenticity\"]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private bool ParseFraudDetectionResponse(string responseText)
        {
            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini fraud detection response");
                    return false;
                }
                
                var jsonResponse = jsonMatch.Value;
                
                // Use case-insensitive deserialization and allow trailing commas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                
                // Parse using JsonDocument to handle the dynamic response
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;
                
                // Check if possibleFraud is true
                bool possibleFraud = false;
                if (root.TryGetProperty("possibleFraud", out var possibleFraudElement))
                {
                    if (possibleFraudElement.ValueKind == JsonValueKind.True)
                    {
                        possibleFraud = true;
                    }
                }
                
                // Check risk level as a backup indicator
                if (!possibleFraud && 
                    root.TryGetProperty("riskLevel", out var riskLevelElement) && 
                    riskLevelElement.ValueKind == JsonValueKind.String)
                {
                    var riskLevel = riskLevelElement.GetString()?.ToLowerInvariant() ?? "low";
                    if (riskLevel == "high" || riskLevel == "medium")
                    {
                        possibleFraud = true;
                    }
                }
                
                return possibleFraud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing fraud detection response");
                return false;
            }
        }

        private bool ParseEnhancedFraudDetectionResponse(string responseText, ValidationResult result)
        {
            bool possibleFraud = false;
            
            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini enhanced fraud detection response");
                    return false;
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
                
                // Initialize fraud detection object with correct properties
                result.FraudDetection = new FraudDetection
                {
                    RiskLevel = FraudRiskLevel.Low,
                    FraudRiskScore = 0,
                    DetectedIndicators = new List<FraudIndicator>(),
                    RecommendedAction = "No action required",
                    RequiresManualReview = false,
                    SuggestedVerificationSteps = new List<string>()
                };
                
                // Extract fraud risk score
                int fraudRiskScore = 0;
                if (root.TryGetProperty("fraudRiskScore", out var riskScoreElement) &&
                    riskScoreElement.ValueKind == JsonValueKind.Number)
                {
                    fraudRiskScore = riskScoreElement.GetInt32();
                    result.FraudDetection.FraudRiskScore = fraudRiskScore;
                }

                // Set RiskLevel based on score
                result.FraudDetection.RiskLevel = fraudRiskScore switch
                {
                    < 25 => FraudRiskLevel.Low,
                    < 50 => FraudRiskLevel.Medium,
                    < 75 => FraudRiskLevel.High,
                    _ => FraudRiskLevel.Critical
                };
                
                // Check if possibleFraud is true
                if (root.TryGetProperty("possibleFraud", out var possibleFraudElement) &&
                    possibleFraudElement.ValueKind == JsonValueKind.True)
                {
                    possibleFraud = true;
                }
                
                // Check risk level as a backup indicator
                string riskLevel = "Low";
                if (root.TryGetProperty("riskLevel", out var riskLevelElement) && 
                    riskLevelElement.ValueKind == JsonValueKind.String)
                {
                    riskLevel = riskLevelElement.GetString() ?? "Low";
                    
                    // If risk level is high or medium, consider it possible fraud
                    if (riskLevel.Equals("High", StringComparison.OrdinalIgnoreCase) || 
                        riskLevel.Equals("Medium", StringComparison.OrdinalIgnoreCase))
                    {
                        possibleFraud = true;
                    }
                }
                
                // Get recommended action
                if (root.TryGetProperty("recommendedAction", out var actionElement) && 
                    actionElement.ValueKind == JsonValueKind.String)
                {
                    result.FraudDetection.RecommendedAction = actionElement.GetString() ?? string.Empty;
                }
                
                // Get requires manual review
                if (root.TryGetProperty("requiresManualReview", out var reviewElement) &&
                    reviewElement.ValueKind == JsonValueKind.True)
                {
                    result.FraudDetection.RequiresManualReview = true;
                }
                
                // Get summary for detailed reasoning
                if (root.TryGetProperty("summary", out var summaryElement) && 
                    summaryElement.ValueKind == JsonValueKind.String)
                {
                    // Store in DetailedReasoning since Notes doesn't exist
                    result.DetailedReasoning = summaryElement.GetString() ?? string.Empty;
                }
                
                // Get detected indicators
                if (root.TryGetProperty("detectedIndicators", out var indicatorsElement) && 
                    indicatorsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var indicator in indicatorsElement.EnumerateArray())
                    {
                        var fraudIndicator = new FraudIndicator();
                        
                        if (indicator.TryGetProperty("indicatorName", out var nameElement) && 
                            nameElement.ValueKind == JsonValueKind.String)
                        {
                            // Use IndicatorName instead of Name
                            fraudIndicator.IndicatorName = nameElement.GetString() ?? string.Empty;
                        }
                        
                        if (indicator.TryGetProperty("description", out var descElement) && 
                            descElement.ValueKind == JsonValueKind.String)
                        {
                            // Use Description instead of Reference
                            fraudIndicator.Description = descElement.GetString() ?? string.Empty;
                        }
                        
                        if (indicator.TryGetProperty("severity", out var severityElement))
                        {
                            if (severityElement.ValueKind == JsonValueKind.Number)
                            {
                                // Convert from 0.0-1.0 to 0-1 double
                                fraudIndicator.Severity = severityElement.GetDouble();
                            }
                        }
                        
                        if (indicator.TryGetProperty("evidence", out var evidenceElement) && 
                            evidenceElement.ValueKind == JsonValueKind.String)
                        {
                            fraudIndicator.Evidence = evidenceElement.GetString() ?? string.Empty;
                        }
                        
                        if (indicator.TryGetProperty("category", out var categoryElement) && 
                            categoryElement.ValueKind == JsonValueKind.String)
                        {
                            // Parse category string to enum
                            var categoryStr = categoryElement.GetString() ?? string.Empty;
                            if (Enum.TryParse<FraudIndicatorCategory>(categoryStr, true, out var category))
                            {
                                fraudIndicator.Category = category;
                            }
                        }
                        
                        if (indicator.TryGetProperty("confidence", out var confidenceElement) &&
                            confidenceElement.ValueKind == JsonValueKind.Number)
                        {
                            fraudIndicator.Confidence = confidenceElement.GetDouble();
                        }
                        
                        // Add the indicator to the list if it has a name
                        if (!string.IsNullOrEmpty(fraudIndicator.IndicatorName))
                        {
                            result.FraudDetection.DetectedIndicators.Add(fraudIndicator);
                        }
                    }
                    
                    // If we have detected indicators but possibleFraud is false, check severity
                    if (!possibleFraud && result.FraudDetection.DetectedIndicators.Count > 0)
                    {
                        // If any indicator has high severity, consider it possible fraud
                        possibleFraud = result.FraudDetection.DetectedIndicators.Any(i => i.Severity > 0.7);
                    }
                }
                
                // Get suggested verification steps
                if (root.TryGetProperty("suggestedVerificationSteps", out var stepsElement) && 
                    stepsElement.ValueKind == JsonValueKind.Array)
                {
                    var steps = new List<string>();
                    foreach (var step in stepsElement.EnumerateArray())
                    {
                        if (step.ValueKind == JsonValueKind.String)
                        {
                            steps.Add(step.GetString() ?? string.Empty);
                        }
                    }
                    
                    if (steps.Count > 0)
                    {
                        // Add to SuggestedVerificationSteps
                        result.FraudDetection.SuggestedVerificationSteps = steps;
                        
                        // Also add to the recommended action for backward compatibility
                        result.FraudDetection.RecommendedAction += 
                            $" Suggested verification steps: {string.Join("; ", steps)}";
                    }
                }
                
                return possibleFraud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing enhanced fraud detection response");
                
                // Initialize fraud detection with error information
                result.FraudDetection = new FraudDetection
                {
                    RiskLevel = FraudRiskLevel.Low,
                    FraudRiskScore = 0,
                    RecommendedAction = $"Error parsing fraud detection response: {ex.Message}"
                };
                
                return false;
            }
        }

        #endregion
    }
}
