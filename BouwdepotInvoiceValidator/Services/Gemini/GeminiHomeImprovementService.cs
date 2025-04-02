using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for validating if invoices are related to home improvement using Gemini AI
    /// </summary>
    public class GeminiHomeImprovementService : GeminiServiceBase
    {
        public GeminiHomeImprovementService(ILogger<GeminiHomeImprovementService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }

        /// <summary>
        /// Uses Gemini AI to validate if the invoice is related to home improvement
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with Gemini's assessment</returns>
        public async Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice)
        {
            _logger.LogInformation("Validating if invoice is related to home improvement");
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Prepare the invoice data for Gemini
                var prompt = BuildHomeImprovementPrompt(invoice);
                
                // Call Gemini API with text-only prompt
                var response = await CallGeminiApiAsync(prompt, null, "HomeImprovementValidation");
                result.RawGeminiResponse = response;
                
                // Parse the response
                var isHomeImprovement = ParseHomeImprovementResponse(response, result);
                result.IsHomeImprovement = isHomeImprovement;
                
                if (!isHomeImprovement)
                {
                    result.AddIssue(ValidationSeverity.Error, 
                        "The invoice does not appear to be related to home improvement expenses.");
                    result.IsValid = false;
                }
                else
                {
                    result.AddIssue(ValidationSeverity.Info, 
                        "The invoice appears to be valid home improvement expenses.");
                    result.IsValid = true;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating home improvement with Gemini");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating with Gemini: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        /// <summary>
        /// Uses Gemini AI with multi-modal capabilities to validate invoice using both text and images
        /// </summary>
        /// <param name="invoice">The extracted invoice data including page images</param>
        /// <returns>A comprehensive validation result with visual and textual analysis</returns>
        public async Task<ValidationResult> ValidateWithMultiModalAnalysisAsync(Invoice invoice)
        {
            _logger.LogInformation("Performing multi-modal validation of invoice with text and images");
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Make sure we have page images
                if (invoice.PageImages == null || invoice.PageImages.Count == 0)
                {
                    _logger.LogWarning("Multi-modal validation requested but no page images are available");
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Multi-modal validation was not possible because no page images were available.");
                    
                    // Fallback to text-only validation
                    return await ValidateHomeImprovementAsync(invoice);
                }
                
                // Prepare multi-modal prompt with both text and images
                var prompt = BuildMultiModalPrompt(invoice);
                
                // Call Gemini API with multi-modal content
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "MultiModalValidation");
                result.RawGeminiResponse = response;
                
                // Parse the response
                ParseMultiModalResponse(response, result);
                
                _logger.LogInformation("Multi-modal validation completed: IsValid={IsValid}, IsHomeImprovement={IsHomeImprovement}", 
                    result.IsValid, result.IsHomeImprovement);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in multi-modal validation with Gemini");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating with multi-modal analysis: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        #region Helper Methods

        private string BuildHomeImprovementPrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### HOME IMPROVEMENT VALIDATION ###");
            promptBuilder.AppendLine("You are a specialized validator for Dutch construction and home improvement expenses.");
            promptBuilder.AppendLine("Your task is to determine if this invoice is related to home improvement expenses.");
            promptBuilder.AppendLine("Home improvement includes construction, renovation, repair, and maintenance of buildings or homes.");
            
            // Add invoice metadata
            promptBuilder.AppendLine("\n### INVOICE METADATA ###");
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
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"category\": \"Category of home improvement or non-home improvement\",");
            promptBuilder.AppendLine("  \"explanation\": \"Detailed explanation of your assessment\",");
            promptBuilder.AppendLine("  \"keywords\": [\"Keywords that support your assessment\"]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private string BuildMultiModalPrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### MULTI-MODAL INVOICE VALIDATION ###");
            promptBuilder.AppendLine("You are a specialized validator for Dutch construction and home improvement expenses.");
            promptBuilder.AppendLine("Your task is to analyze this invoice using both text and visual information to determine:");
            promptBuilder.AppendLine("1. If it's a valid invoice document");
            promptBuilder.AppendLine("2. If it's related to home improvement expenses");
            promptBuilder.AppendLine("3. If there are any visual indicators of fraud or tampering");
            
            // Add invoice metadata
            promptBuilder.AppendLine("\n### INVOICE METADATA ###");
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
            
            // Visual assessment instructions
            promptBuilder.AppendLine("\n### VISUAL ASSESSMENT INSTRUCTIONS ###");
            promptBuilder.AppendLine("- Look for visual signs of tampering or alterations");
            promptBuilder.AppendLine("- Check for inconsistencies in formatting, fonts, or alignment");
            promptBuilder.AppendLine("- Verify that the document appears to be a legitimate invoice");
            promptBuilder.AppendLine("- Identify construction or home improvement related visual elements");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isValid\": true/false,");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidenceScore\": 0-100,");
            promptBuilder.AppendLine("  \"visualIndicators\": [\"Visual elements that support your analysis\"],");
            promptBuilder.AppendLine("  \"possibleFraud\": true/false,");
            promptBuilder.AppendLine("  \"fraudIndicators\": [\"Any visual indicators of fraud\"],");
            promptBuilder.AppendLine("  \"explanation\": \"Detailed explanation of your assessment\"");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private bool ParseHomeImprovementResponse(string responseText, ValidationResult result)
        {
            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini home improvement response");
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Failed to parse AI response format. Assuming this is not home improvement related.");
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
                
                // Check if isHomeImprovement is true
                bool isHomeImprovement = false;
                if (root.TryGetProperty("isHomeImprovement", out var isHomeImprovementElement))
                {
                    if (isHomeImprovementElement.ValueKind == JsonValueKind.True)
                    {
                        isHomeImprovement = true;
                    }
                }
                
                // Get confidence level
                double confidence = 0;
                if (root.TryGetProperty("confidence", out var confidenceElement) && 
                    confidenceElement.ValueKind == JsonValueKind.Number)
                {
                    confidence = confidenceElement.GetDouble();
                    result.ConfidenceScore = (int)(confidence * 100);
                }
                
                // Get category
                string category = "Unclassified";
                if (root.TryGetProperty("category", out var categoryElement) && 
                    categoryElement.ValueKind == JsonValueKind.String)
                {
                    category = categoryElement.GetString() ?? "Unclassified";
                    
                    if (result.PurchaseAnalysis == null)
                    {
                        result.PurchaseAnalysis = new PurchaseAnalysis();
                    }

                    if (!string.IsNullOrEmpty(category))
                    {
                        result.PurchaseAnalysis.Categories = new List<string> { category };
                        result.PurchaseAnalysis.PrimaryPurpose = category;
                    }
                }
                
                // Get explanation
                string explanation = string.Empty;
                if (root.TryGetProperty("explanation", out var explanationElement) && 
                    explanationElement.ValueKind == JsonValueKind.String)
                {
                    explanation = explanationElement.GetString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(explanation))
                    {
                        // Add the explanation as an informational message
                        result.AddIssue(ValidationSeverity.Info, explanation);
                        
                        // Update PurchaseAnalysis.Summary if available
                        if (result.PurchaseAnalysis != null)
                        {
                            result.PurchaseAnalysis.Summary = explanation;
                        }
                    }
                }
                
                // Get keywords
                if (root.TryGetProperty("keywords", out var keywordsElement) && 
                    keywordsElement.ValueKind == JsonValueKind.Array)
                {
                    var keywords = new List<string>();
                    foreach (var keywordElement in keywordsElement.EnumerateArray())
                    {
                        if (keywordElement.ValueKind == JsonValueKind.String)
                        {
                            keywords.Add(keywordElement.GetString() ?? string.Empty);
                        }
                    }
                    
                    if (keywords.Count > 0)
                    {
                        // Display the keywords as additional info
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Identified keywords: {string.Join(", ", keywords)}");
                    }
                }
                
                // Add a summary message based on the confidence level
                if (isHomeImprovement)
                {
                    string confidenceText = confidence switch
                    {
                        > 0.9 => "very high confidence",
                        > 0.7 => "high confidence",
                        > 0.5 => "moderate confidence",
                        _ => "low confidence"
                    };
                    
                    result.AddIssue(ValidationSeverity.Info, 
                        $"This invoice is related to home improvement ({confidenceText}).");
                    
                    if (!string.IsNullOrEmpty(category))
                    {
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Category: {category}");
                    }
                }
                else
                {
                    result.AddIssue(ValidationSeverity.Warning, 
                        $"This invoice does not appear to be related to home improvement. {(category != "Unclassified" ? $"Category: {category}" : "")}");
                }
                
                return isHomeImprovement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing home improvement response");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error analyzing home improvement relevance: {ex.Message}");
                return false;
            }
        }

        private void ParseMultiModalResponse(string responseText, ValidationResult result)
        {
            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini multi-modal response");
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Failed to parse AI response format. Multi-modal analysis failed.");
                    result.IsValid = false;
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
                
                // Parse using JsonDocument
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;
                
                // Extract properties from the response
                
                // Is valid invoice
                if (root.TryGetProperty("isValid", out var isValidElement) &&
                    isValidElement.ValueKind == JsonValueKind.True)
                {
                    result.IsValid = true;
                    result.IsVerifiedInvoice = true;
                }
                
                // Is home improvement
                if (root.TryGetProperty("isHomeImprovement", out var isHomeImprovementElement) &&
                    isHomeImprovementElement.ValueKind == JsonValueKind.True)
                {
                    result.IsHomeImprovement = true;
                }
                
                // Confidence score
                if (root.TryGetProperty("confidenceScore", out var confidenceElement) &&
                    confidenceElement.ValueKind == JsonValueKind.Number)
                {
                    result.ConfidenceScore = confidenceElement.GetInt32();
                }
                
                // Possible fraud
                bool possibleFraud = false;
                if (root.TryGetProperty("possibleFraud", out var possibleFraudElement) &&
                    possibleFraudElement.ValueKind == JsonValueKind.True)
                {
                    possibleFraud = true;
                    result.PossibleTampering = true;
                }
                
                // Visual indicators
                if (root.TryGetProperty("visualIndicators", out var visualIndicatorsElement) &&
                    visualIndicatorsElement.ValueKind == JsonValueKind.Array)
                {
                    List<string> visualIndicators = new List<string>();
                    foreach (var indicator in visualIndicatorsElement.EnumerateArray())
                    {
                        if (indicator.ValueKind == JsonValueKind.String)
                        {
                            visualIndicators.Add(indicator.GetString() ?? string.Empty);
                        }
                    }
                    
                    if (visualIndicators.Count > 0)
                    {
                        // Store visual indicators as visual assessments
                        result.VisualAssessments = new List<VisualAssessment>();
                        foreach (var indicator in visualIndicators)
                        {
                            result.VisualAssessments.Add(new VisualAssessment
                            {
                                ElementName = indicator,
                                Evidence = indicator,
                                Score = result.IsValid ? 1 : -1,
                                Confidence = result.ConfidenceScore
                            });
                        }
                        
                        // Also add as info message
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Visual indicators: {string.Join(", ", visualIndicators)}");
                    }
                }
                
                // Fraud indicators
                if (root.TryGetProperty("fraudIndicators", out var fraudIndicatorsElement) &&
                    fraudIndicatorsElement.ValueKind == JsonValueKind.Array)
                {
                    List<string> fraudIndicators = new List<string>();
                    foreach (var indicator in fraudIndicatorsElement.EnumerateArray())
                    {
                        if (indicator.ValueKind == JsonValueKind.String)
                        {
                            fraudIndicators.Add(indicator.GetString() ?? string.Empty);
                        }
                    }
                    
                    if (fraudIndicators.Count > 0)
                    {
                        // Add fraud detection
                        result.FraudDetection = new FraudDetection
                        {
                            RiskLevel = possibleFraud ? 70 : 30,
                            FraudIndicatorsDetected = possibleFraud,
                            DetectedIndicators = new List<FraudIndicator>()
                        };
                        
                        // Add indicators
                        foreach (var indicator in fraudIndicators)
                        {
                            result.FraudDetection.DetectedIndicators.Add(new FraudIndicator
                            {
                                Name = indicator,
                                Evidence = indicator,
                                Severity = possibleFraud ? 70 : 30
                            });
                        }
                        
                        // Also add as info message
                        var severity = possibleFraud ? ValidationSeverity.Warning : ValidationSeverity.Info;
                        result.AddIssue(severity, 
                            $"Fraud indicators: {string.Join(", ", fraudIndicators)}");
                    }
                }
                
                // Explanation
                if (root.TryGetProperty("explanation", out var explanationElement) &&
                    explanationElement.ValueKind == JsonValueKind.String)
                {
                    string explanation = explanationElement.GetString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(explanation))
                    {
                        result.DetailedReasoning = explanation;
                        
                        // Add explanation as info message if we don't have too many issues already
                        if (result.Issues.Count < 5)
                        {
                            result.AddIssue(ValidationSeverity.Info, explanation);
                        }
                    }
                }
                
                // Set overall status messages based on results
                if (!result.IsValid)
                {
                    result.AddIssue(ValidationSeverity.Error, 
                        "The document does not appear to be a valid invoice.");
                }
                else if (!result.IsHomeImprovement)
                {
                    result.AddIssue(ValidationSeverity.Warning, 
                        "The invoice does not appear to be related to home improvement.");
                }
                else if (possibleFraud)
                {
                    result.AddIssue(ValidationSeverity.Warning, 
                        "The invoice shows signs of possible tampering or fraud.");
                }
                else
                {
                    result.AddIssue(ValidationSeverity.Info, 
                        "The document appears to be a valid home improvement invoice.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing multi-modal response");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error analyzing document with multi-modal analysis: {ex.Message}");
                result.IsValid = false;
            }
        }

        #endregion
    }
}
