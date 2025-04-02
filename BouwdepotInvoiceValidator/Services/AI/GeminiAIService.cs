using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Implementation of IAIService using Google's Gemini API
    /// </summary>
    public class GeminiAIService : AIServiceBase
    {
        private readonly string _apiKey;
        private readonly string _projectId;
        private readonly string _location;
        private readonly string _modelId;
        private readonly GeminiImageGenerator _imageGenerator;
        private readonly ILoggerFactory _loggerFactory;
        
        /// <summary>
        /// Creates a new instance of the GeminiAIService
        /// </summary>
        public GeminiAIService(
            ILogger<GeminiAIService> logger,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
            : base(logger, configuration, LoadGeminiOptions(configuration))
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            
            // Load Gemini-specific settings from configuration
            var aiConfig = configuration.GetSection("AI");
            var geminiConfig = aiConfig.GetSection("Providers:Gemini");
            
            _apiKey = geminiConfig["ApiKey"] ?? configuration["Gemini:ApiKey"];
            _projectId = geminiConfig["ProjectId"] ?? configuration["Gemini:ProjectId"];
            _location = geminiConfig["Location"] ?? configuration["Gemini:Location"];
            _modelId = geminiConfig["ModelId"] ?? configuration["Gemini:ModelId"];
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new ArgumentException("Gemini API key is required");
            }
            
            if (string.IsNullOrEmpty(_modelId))
            {
                _modelId = "gemini-2.0-flash"; // Default model
            }
            
            // Initialize the image generator for parallel processing with the correct logger type
            _imageGenerator = new GeminiImageGenerator(
                _loggerFactory.CreateLogger<GeminiImageGenerator>(), 
                configuration);
            
            _logger.LogInformation("GeminiAIService initialized with model: {ModelId}", _modelId);
        }
        
        /// <summary>
        /// Validates an invoice using the Gemini AI model
        /// </summary>
        public override async Task<ValidationResult> ValidateInvoiceAsync(
            Invoice invoice, 
            string locale = "nl-NL", 
            int? confidenceThreshold = null)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }
            
            _logger.LogInformation("Validating invoice using Gemini AI: {InvoiceNumber}", invoice.InvoiceNumber);
            
            try
            {
                // Build the unified validation prompt
                string prompt = BuildUnifiedValidationPrompt(invoice, locale);
                
                // For demonstration purposes, we'll generate a mock response
                // In a real implementation, we would send this prompt to Gemini API
                
                // This is where you would make the actual API call to Gemini
                // var response = await CallGeminiApi(prompt, invoice.PageImages);
                
                // Parse the response JSON (in a real implementation)
                // var jsonResponse = ParseGeminiResponse(response);
                
                // Mock the response for demonstration
                var mockJsonResponse = GenerateMockResponseJson(invoice);
                
                // Create validation result from the response
                var validationResult = CreateValidationResultFromResponse(mockJsonResponse, invoice);
                
                // Set confidence threshold based on parameter or configuration
                int threshold = confidenceThreshold ?? _options.DefaultConfidenceThreshold;
                validationResult.MeetsApprovalThreshold = validationResult.ConfidenceScore >= threshold;
                
                return validationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice with Gemini AI: {InvoiceNumber}", invoice.InvoiceNumber);
                
                var errorResult = new ValidationResult
                {
                    IsValid = false,
                    ExtractedInvoice = invoice
                };
                
                errorResult.AddIssue(ValidationSeverity.Error, 
                    $"Error processing with AI: {ex.Message}");
                
                return errorResult;
            }
        }
        
        /// <summary>
        /// Generates an image from a text prompt with parallel processing support for improved performance
        /// </summary>
        /// <param name="prompt">The text prompt describing the image to generate</param>
        /// <param name="width">Width of the generated image</param>
        /// <param name="height">Height of the generated image</param>
        /// <param name="useCache">Whether to use caching for repeated requests</param>
        /// <returns>The generated image as a byte array</returns>
        public async Task<byte[]> GenerateImageAsync(string prompt, int width = 1024, int height = 1024, bool useCache = true)
        {
            _logger.LogInformation("Generating image with prompt: {PromptPreview}", 
                prompt.Length > 50 ? prompt.Substring(0, 50) + "..." : prompt);
            
            return await _imageGenerator.GenerateImageFromTextAsync(prompt, width, height, useCache);
        }
        
        /// <summary>
        /// Generates multiple images in parallel with controlled concurrency
        /// </summary>
        /// <param name="prompts">Collection of text prompts</param>
        /// <param name="width">Width of the generated images</param>
        /// <param name="height">Height of the generated images</param>
        /// <param name="useCache">Whether to use caching</param>
        /// <returns>Dictionary mapping prompts to generated images</returns>
        public async Task<Dictionary<string, byte[]>> GenerateImagesInParallelAsync(
            IEnumerable<string> prompts, 
            int width = 1024, 
            int height = 1024, 
            bool useCache = true)
        {
            _logger.LogInformation("Generating {Count} images in parallel", prompts.Count());
            
            return await _imageGenerator.GenerateImagesInParallelAsync(prompts, width, height, useCache);
        }
        
        /// <summary>
        /// Clears the image cache to free up memory
        /// </summary>
        public void ClearImageCache()
        {
            _imageGenerator.ClearCache();
        }
        
        /// <summary>
        /// Gets information about the Gemini model
        /// </summary>
        public override AIModelInfo GetModelInfo()
        {
            return new AIModelInfo
            {
                ProviderName = "Google",
                ModelId = _modelId,
                ModelVersion = GetModelVersion(_modelId),
                MaxContextLength = GetMaxContextLength(_modelId),
                SupportsMultiModal = SupportsMultiModal(_modelId),
                LastUpdated = GetModelLastUpdated(_modelId)
            };
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Loads Gemini-specific options from configuration
        /// </summary>
        private static AIServiceOptions LoadGeminiOptions(IConfiguration configuration)
        {
            var aiConfig = configuration.GetSection("AI");
            
            return new AIServiceOptions
            {
                Temperature = aiConfig.GetValue<double>("Temperature", 0.2),
                UseMultiModalAnalysis = aiConfig.GetValue<bool>("UseMultiModal", true),
                MaxOutputTokens = aiConfig.GetValue<int>("MaxOutputTokens", 4096),
                DefaultConfidenceThreshold = configuration.GetValue<int>("Validation:ConfidenceThreshold", 75),
                EnableAutoApproval = configuration.GetValue<bool>("Validation:EnableAutoApproval", true),
                HighRiskThreshold = 40
            };
        }
        
        /// <summary>
        /// Gets the version of the specified Gemini model
        /// </summary>
        private string GetModelVersion(string modelId)
        {
            // Extract version from model ID
            if (modelId.Contains("gemini-2.0"))
            {
                return "2.0";
            }
            else if (modelId.Contains("gemini-1.5"))
            {
                return "1.5";
            }
            else if (modelId.Contains("gemini-1.0"))
            {
                return "1.0";
            }
            
            return "Unknown";
        }
        
        /// <summary>
        /// Gets the maximum context length for the specified model
        /// </summary>
        private int GetMaxContextLength(string modelId)
        {
            // Different models have different context lengths
            return modelId switch
            {
                var m when m.Contains("gemini-2.0-flash") => 2048,
                var m when m.Contains("gemini-2.0-pro") => 32000,
                var m when m.Contains("gemini-1.5-flash") => 16000,
                var m when m.Contains("gemini-1.5-pro") => 32000,
                _ => 16000 // Default
            };
        }
        
        /// <summary>
        /// Gets whether the specified model supports multi-modal inputs
        /// </summary>
        private bool SupportsMultiModal(string modelId)
        {
            // Most Gemini models support multi-modal inputs
            return modelId.Contains("gemini");
        }
        
        /// <summary>
        /// Gets the last updated date for the specified model
        /// </summary>
        private string GetModelLastUpdated(string modelId)
        {
            // This information would typically come from the provider's documentation
            return modelId switch
            {
                var m when m.Contains("gemini-2.0") => "2024-01",
                var m when m.Contains("gemini-1.5") => "2023-10",
                var m when m.Contains("gemini-1.0") => "2023-06",
                _ => "Unknown"
            };
        }
        
        /// <summary>
        /// Generates a mock response for demonstration purposes
        /// In a real implementation, this would be the actual response from the Gemini API
        /// </summary>
        private string GenerateMockResponseJson(Invoice invoice)
        {
            // Create mock response based on invoice contents
            bool hasHomeImprovementKeywords = false;
            bool hasNonHomeImprovementKeywords = false;
            int confidenceScore = 75; // Default
            
            var categories = new HashSet<string>();
            var confidenceFactors = new List<object>();
            var fraudIndicators = new List<object>();
            
            // Simple keyword-based analysis for demo purposes
            if (invoice.LineItems.Count > 0)
            {
                var homeImprovementKeywords = new[] 
                { 
                    "renovation", "kitchen", "bathroom", "wall", "floor", "ceiling", "paint", 
                    "plumbing", "electrical", "window", "door", "roof", "install", "tile", 
                    "cabinet", "counter", "sink", "toilet", "shower" 
                };
                
                var nonHomeImprovementKeywords = new[]
                {
                    "furniture", "appliance", "decoration", "curtain", "rug", "lamp",
                    "cleaning", "maintenance", "repair", "mobile", "temporary"
                };
                
                int homeImprovementCount = 0;
                int nonHomeImprovementCount = 0;
                
                foreach (var item in invoice.LineItems)
                {
                    if (string.IsNullOrEmpty(item.Description))
                        continue;
                        
                    if (homeImprovementKeywords.Any(k => item.Description.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        homeImprovementCount++;
                        hasHomeImprovementKeywords = true;
                        
                        // Add potential categories
                        if (item.Description.Contains("kitchen", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Kitchen Renovation");
                        else if (item.Description.Contains("bathroom", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Bathroom Renovation");
                        else if (item.Description.Contains("wall", StringComparison.OrdinalIgnoreCase) || 
                                item.Description.Contains("paint", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Interior Decoration");
                        else if (item.Description.Contains("floor", StringComparison.OrdinalIgnoreCase) || 
                                item.Description.Contains("tile", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Flooring");
                        else if (item.Description.Contains("plumbing", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Plumbing");
                        else if (item.Description.Contains("electrical", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Electrical Work");
                        else if (item.Description.Contains("window", StringComparison.OrdinalIgnoreCase) || 
                                item.Description.Contains("door", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Windows and Doors");
                        else if (item.Description.Contains("roof", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Roofing");
                        else
                            categories.Add("General Construction");
                    }
                    
                    if (nonHomeImprovementKeywords.Any(k => item.Description.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        nonHomeImprovementCount++;
                        hasNonHomeImprovementKeywords = true;
                        
                        if (item.Description.Contains("furniture", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Furniture");
                        else if (item.Description.Contains("appliance", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Appliances");
                        else if (item.Description.Contains("decoration", StringComparison.OrdinalIgnoreCase) || 
                                item.Description.Contains("curtain", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Decoration");
                        else if (item.Description.Contains("cleaning", StringComparison.OrdinalIgnoreCase) || 
                                item.Description.Contains("maintenance", StringComparison.OrdinalIgnoreCase))
                            categories.Add("Maintenance");
                    }
                }
                
                // Calculate confidence score based on the ratio of home improvement to non-home improvement items
                if (homeImprovementCount + nonHomeImprovementCount > 0)
                {
                    double ratio = (double)homeImprovementCount / (homeImprovementCount + nonHomeImprovementCount);
                    confidenceScore = (int)(ratio * 100);
                    
                    // Add confidence factors
                    if (homeImprovementCount > 0)
                    {
                        confidenceFactors.Add(new 
                        {
                            factorName = $"Contains {homeImprovementCount} home improvement items",
                            impact = Math.Min(homeImprovementCount * 5, 20),
                            explanation = "Line items contain terms commonly associated with home improvement"
                        });
                    }
                    
                    if (nonHomeImprovementCount > 0)
                    {
                        confidenceFactors.Add(new 
                        {
                            factorName = $"Contains {nonHomeImprovementCount} non-home improvement items",
                            impact = -Math.Min(nonHomeImprovementCount * 5, 20),
                            explanation = "Line items contain terms not typically associated with permanent home improvements"
                        });
                    }
                    
                    // Add potential fraud indicators
                    if (invoice.LineItems.Any(item => item.TotalPrice > 5000))
                    {
                        fraudIndicators.Add(new 
                        {
                            indicatorName = "High value line item",
                            description = "Line item with unusually high value detected",
                            evidence = "Line item exceeds €5000",
                            severity = 0.3
                        });
                    }
                    
                    if (invoice.LineItems.All(item => item.TotalPrice % 100 == 0))
                    {
                        fraudIndicators.Add(new 
                        {
                            indicatorName = "Rounded prices",
                            description = "All prices are rounded to even hundreds",
                            evidence = "Pattern of exact round numbers",
                            severity = 0.4
                        });
                    }
                }
            }
            
            // Determine if it's a home improvement invoice
            bool isHomeImprovement = confidenceScore >= 60;
            
            // Create the mock response object
            var response = new
            {
                isValidInvoice = true,
                isHomeImprovement = isHomeImprovement,
                confidenceScore = confidenceScore,
                contentSummary = new 
                {
                    purchasedItems = string.Join(", ", invoice.LineItems.Select(i => i.Description).Take(3)),
                    intendedPurpose = isHomeImprovement ? "Home renovation and improvement" : "Mixed purposes including non-permanent items",
                    propertyImpact = isHomeImprovement ? "Likely to increase property value through permanent improvements" : "Limited impact on property value",
                    projectCategory = categories.FirstOrDefault() ?? "General Expenditure",
                    estimatedProjectScope = invoice.TotalAmount > 5000 ? "Major renovation" : "Minor improvements"
                },
                confidenceFactors = confidenceFactors,
                fraudDetection = new 
                {
                    fraudRiskScore = confidenceScore < 40 ? 60 : 20,
                    recommendedAction = confidenceScore < 40 ? "Request additional verification" : "Approve",
                    fraudIndicators = fraudIndicators
                },
                auditReport = new 
                {
                    executiveSummary = isHomeImprovement 
                        ? "The invoice contains valid home improvement expenses eligible for reimbursement" 
                        : "The invoice contains some items that may not qualify as permanent home improvements",
                    ruleAssessments = new[] 
                    {
                        new 
                        {
                            ruleName = "Permanent Attachment",
                            description = "Items must be permanently attached to the property",
                            isSatisfied = hasHomeImprovementKeywords && !hasNonHomeImprovementKeywords,
                            evidence = hasHomeImprovementKeywords ? "Contains items typically permanently installed" : "No clear evidence of permanent installation",
                            reasoning = "Based on line item descriptions and common home improvement patterns",
                            score = hasHomeImprovementKeywords && !hasNonHomeImprovementKeywords ? 90 : 50
                        },
                        new 
                        {
                            ruleName = "Quality Improvement",
                            description = "Improvements must enhance property value or quality",
                            isSatisfied = hasHomeImprovementKeywords,
                            evidence = hasHomeImprovementKeywords ? "Items typically improve property quality" : "Limited evidence of property improvement",
                            reasoning = "Based on category of purchased items and industry standards",
                            score = hasHomeImprovementKeywords ? 85 : 40
                        }
                    },
                    approvalFactors = new List<string>(),
                    concernFactors = new List<string>()
                }
            };
            
            // Add approval and concern factors based on our analysis
            if (hasHomeImprovementKeywords)
            {
                ((List<string>)response.auditReport.approvalFactors).Add("Contains recognized home improvement items");
            }
            
            if (hasNonHomeImprovementKeywords)
            {
                ((List<string>)response.auditReport.concernFactors).Add("Contains items not typically considered permanent improvements");
            }
            
            if (invoice.TotalAmount > 10000)
            {
                ((List<string>)response.auditReport.concernFactors).Add("Unusually high total amount");
            }
            
            // Serialize the mock response to JSON
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        
        /// <summary>
        /// Creates a ValidationResult from the Gemini API response JSON
        /// </summary>
        private ValidationResult CreateValidationResultFromResponse(string responseJson, Invoice invoice)
        {
            try
            {
                // Parse the JSON response
                var jsonDoc = JsonDocument.Parse(responseJson);
                var root = jsonDoc.RootElement;
                
                // Create the validation result
                var result = new ValidationResult
                {
                    IsValid = GetJsonBoolValue(root, "isValidInvoice", true),
                    IsHomeImprovement = GetJsonBoolValue(root, "isHomeImprovement", false),
                    ConfidenceScore = GetJsonIntValue(root, "confidenceScore", 0),
                    ExtractedInvoice = invoice,
                    RawAIResponse = responseJson,
                    ValidatedAt = DateTime.UtcNow
                };
                
                // Extract content summary
                if (root.TryGetProperty("contentSummary", out var summaryElement))
                {
                    result.Summary = new ContentSummary
                    {
                        PurchasedItems = GetJsonStringValue(summaryElement, "purchasedItems"),
                        IntendedPurpose = GetJsonStringValue(summaryElement, "intendedPurpose"),
                        PropertyImpact = GetJsonStringValue(summaryElement, "propertyImpact"),
                        ProjectCategory = GetJsonStringValue(summaryElement, "projectCategory"),
                        EstimatedProjectScope = GetJsonStringValue(summaryElement, "estimatedProjectScope")
                    };
                }
                
                // Extract confidence factors
                if (root.TryGetProperty("confidenceFactors", out var factorsElement) && factorsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var factor in factorsElement.EnumerateArray())
                    {
                        string factorName = GetJsonStringValue(factor, "factorName");
                        int impact = GetJsonIntValue(factor, "impact", 0);
                        string explanation = GetJsonStringValue(factor, "explanation");
                        
                        result.AddConfidenceFactor(factorName, impact, explanation);
                    }
                }
                
                // Extract fraud detection
                if (root.TryGetProperty("fraudDetection", out var fraudElement))
                {
                    result.FraudDetection = new FraudDetection
                    {
                        FraudRiskScore = GetJsonIntValue(fraudElement, "fraudRiskScore", 0),
                        RecommendedAction = GetJsonStringValue(fraudElement, "recommendedAction")
                    };
                    
                    // Set risk level based on score
                    result.FraudDetection.RiskLevel = result.FraudDetection.FraudRiskScore switch
                    {
                        < 25 => FraudRiskLevel.Low,
                        < 50 => FraudRiskLevel.Medium,
                        < 75 => FraudRiskLevel.High,
                        _ => FraudRiskLevel.Critical
                    };
                    
                    // Extract fraud indicators
                    if (fraudElement.TryGetProperty("fraudIndicators", out var indicatorsElement) && 
                        indicatorsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var indicator in indicatorsElement.EnumerateArray())
                        {
                            string indicatorName = GetJsonStringValue(indicator, "indicatorName");
                            string description = GetJsonStringValue(indicator, "description");
                            string evidence = GetJsonStringValue(indicator, "evidence");
                            double severity = GetJsonDoubleValue(indicator, "severity", 0.0);
                            
                            result.FraudDetection.DetectedIndicators.Add(new BouwdepotInvoiceValidator.Models.FraudIndicator
                            {
                                IndicatorName = indicatorName,
                                Description = description,
                                Evidence = evidence,
                                Severity = severity,
                                Category = (BouwdepotInvoiceValidator.Models.FraudIndicatorCategory)FraudIndicatorCategory.DocumentIssue
                            });
                        }
                    }
                }
                
                // Extract audit report
                if (root.TryGetProperty("auditReport", out var auditElement))
                {
                    result.AuditReport = new AuditReport
                    {
                        ExecutiveSummary = GetJsonStringValue(auditElement, "executiveSummary")
                    };
                    
                    // Extract rule assessments
                    if (auditElement.TryGetProperty("ruleAssessments", out var rulesElement) && 
                        rulesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var rule in rulesElement.EnumerateArray())
                        {
                            string ruleName = GetJsonStringValue(rule, "ruleName");
                            string description = GetJsonStringValue(rule, "description");
                            bool isSatisfied = GetJsonBoolValue(rule, "isSatisfied", false);
                            string evidence = GetJsonStringValue(rule, "evidence");
                            string reasoning = GetJsonStringValue(rule, "reasoning");
                            int score = GetJsonIntValue(rule, "score", 0);
                            
                            result.AuditReport.RuleAssessments.Add(new RuleAssessment
                            {
                                RuleName = ruleName,
                                Description = description,
                                IsSatisfied = isSatisfied,
                                Evidence = evidence,
                                Reasoning = reasoning,
                                Score = score
                            });
                        }
                    }
                    
                    // Extract approval factors
                    if (auditElement.TryGetProperty("approvalFactors", out var approvalElement) && 
                        approvalElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var factor in approvalElement.EnumerateArray())
                        {
                            result.AuditReport.ApprovalFactors.Add(factor.GetString());
                        }
                    }
                    
                    // Extract concern factors
                    if (auditElement.TryGetProperty("concernFactors", out var concernElement) && 
                        concernElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var factor in concernElement.EnumerateArray())
                        {
                            result.AuditReport.ConcernFactors.Add(factor.GetString());
                        }
                    }
                }
                
                // Add important validation issues based on the response
                if (result.IsValid && result.IsHomeImprovement)
                {
                    result.AddIssue(ValidationSeverity.Info, 
                        $"Valid home improvement invoice with {result.ConfidenceScore}% confidence");
                }
                else if (result.IsValid && !result.IsHomeImprovement)
                {
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Valid invoice but does not appear to be for home improvements");
                }
                else if (!result.IsValid)
                {
                    result.AddIssue(ValidationSeverity.Error, 
                        "Document does not appear to be a valid invoice");
                }
                
                // Add fraud risk information if detected
                if (result.FraudDetection.FraudRiskScore > _options.HighRiskThreshold)
                {
                    var severity = result.FraudDetection.FraudRiskScore > 60 
                        ? ValidationSeverity.Error 
                        : ValidationSeverity.Warning;
                    
                    result.AddIssue(severity, 
                        $"Potential fraud risk detected (score: {result.FraudDetection.FraudRiskScore}/100). " +
                        $"Recommended action: {result.FraudDetection.RecommendedAction}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini response JSON");
                
                // Create a fallback validation result
                var fallbackResult = new ValidationResult
                {
                    IsValid = false,
                    ExtractedInvoice = invoice,
                    RawAIResponse = responseJson,
                    ValidatedAt = DateTime.UtcNow
                };
                
                fallbackResult.AddIssue(ValidationSeverity.Error, 
                    $"Error processing AI response: {ex.Message}");
                
                return fallbackResult;
            }
        }
        
        /// <summary>
        /// Helper method to safely get a boolean value from a JSON element
        /// </summary>
        private bool GetJsonBoolValue(JsonElement element, string propertyName, bool defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            {
                return property.GetBoolean();
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Helper method to safely get a string value from a JSON element
        /// </summary>
        private string GetJsonStringValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString() ?? string.Empty;
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Helper method to safely get an integer value from a JSON element
        /// </summary>
        private int GetJsonIntValue(JsonElement element, string propertyName, int defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number)
                {
                    return property.GetInt32();
                }
                else if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out int result))
                {
                    return result;
                }
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Helper method to safely get a double value from a JSON element
        /// </summary>
        private double GetJsonDoubleValue(JsonElement element, string propertyName, double defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number)
                {
                    return property.GetDouble();
                }
                else if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), out double result))
                {
                    return result;
                }
            }
            
            return defaultValue;
        }
        
        #endregion
    }
}
