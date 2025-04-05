using System.Text.Json;
using System.Text.RegularExpressions;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for analyzing invoice line items using Gemini AI
    /// </summary>
    public class GeminiLineItemAnalysisService : GeminiServiceBase
    {
        public GeminiLineItemAnalysisService(ILogger<GeminiLineItemAnalysisService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }

        /// <summary>
        /// Uses Gemini AI to analyze the invoice line items to determine what was purchased
        /// </summary>
        /// <param name="invoice">The extracted invoice data with line items</param>
        /// <returns>A detailed analysis of the invoice line items</returns>
        public async Task<Models.Analysis.LineItemAnalysisResult> AnalyzeLineItemsAsync(Invoice invoice)
        {
            _logger.LogInformation("Analyzing invoice line items using Gemini AI");

            try
            {
                // Make sure we have line items to analyze
                if (invoice.LineItems == null || invoice.LineItems.Count == 0)
                {
                    _logger.LogWarning("Line item analysis requested but no line items are available");

                    // Return an empty result - Explicitly use Models.Analysis namespace
                    return new Models.Analysis.LineItemAnalysisResult
                    {
                        Categories = new List<string> { "Unknown" },
                        PrimaryCategory = "Unknown",
                        HomeImprovementRelevance = 0,
                        LineItemAnalysis = new List<Models.Analysis.LineItemAnalysisDetails>(),
                        OverallAssessment = "No line items available for analysis",
                        RawResponse = "Line item analysis was not performed due to missing line items"
                    };
                }
                
                // Prepare the line item analysis prompt
                var prompt = BuildLineItemAnalysisPrompt(invoice);
                
                // Call Gemini API with text-only prompt
                var response = await CallGeminiApiAsync(prompt, null, "LineItemAnalysis");
                
                // Parse the response into a structured result
                var result = ParseLineItemAnalysisResponse(response, invoice);
                result.RawResponse = response;
                
                _logger.LogInformation("Line item analysis completed: PrimaryCategory={PrimaryCategory}, HomeImprovementRelevance={Relevance}", 
                    result.PrimaryCategory, result.HomeImprovementRelevance);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing line items with Gemini");

                // Return a minimal result with error information - Explicitly use Models.Analysis namespace
                return new Models.Analysis.LineItemAnalysisResult
                {
                    Categories = new List<string> { "Error" },
                    PrimaryCategory = "Error",
                    HomeImprovementRelevance = 0,
                    LineItemAnalysis = new List<Models.Analysis.LineItemAnalysisDetails>(),
                    OverallAssessment = $"Error analyzing line items: {ex.Message}",
                    RawResponse = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Uses Gemini AI to provide a detailed audit-ready assessment of the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with detailed audit information</returns>
        public async Task<ValidationResult> GetAuditReadyAssessmentAsync(Invoice invoice)
        {
            _logger.LogInformation("Generating audit-ready assessment for invoice");
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Prepare the audit assessment prompt
                var prompt = BuildAuditAssessmentPrompt(invoice);
                
                // Call Gemini API with invoice data
                var response = await CallGeminiApiAsync(
                    prompt, 
                    invoice.PageImages?.Count > 0 ? invoice.PageImages : null, 
                    "AuditAssessment"
                );
                
                result.RawGeminiResponse = response;
                
                // Parse the response and update the validation result
                ParseAuditAssessmentResponse(response, result);
                
                _logger.LogInformation("Completed audit-ready assessment: IsValid={IsValid}", result.IsValid);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit assessment with Gemini");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error generating audit assessment: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        #region Helper Methods
        
        private string BuildLineItemAnalysisPrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### LINE ITEM ANALYSIS ###");
            promptBuilder.AppendLine("You are a construction and home improvement expert.");
            promptBuilder.AppendLine("Your task is to analyze the line items in this invoice and provide a detailed assessment.");
            
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
            
            // Add line items 
            promptBuilder.AppendLine("\n### LINE ITEMS ###");
            foreach (var item in invoice.LineItems)
            {
                promptBuilder.AppendLine($"- {item.Quantity}x {item.Description}: {item.TotalPrice:C}");
            }
            
            // Additional context from raw text if available and not too long
            if (!string.IsNullOrEmpty(invoice.RawText) && invoice.RawText.Length < 500)
            {
                promptBuilder.AppendLine("\n### ADDITIONAL CONTEXT ###");
                promptBuilder.AppendLine(invoice.RawText);
            }
            
            promptBuilder.AppendLine("\n### ANALYSIS INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Analyze each line item to determine:");
            promptBuilder.AppendLine("   - What product or service it represents");
            promptBuilder.AppendLine("   - Whether it's related to home improvement");
            promptBuilder.AppendLine("   - What category it belongs to (e.g., plumbing, electrical, flooring)");
            promptBuilder.AppendLine("   - If pricing appears reasonable");
            promptBuilder.AppendLine("2. Determine the primary purpose of the invoice");
            promptBuilder.AppendLine("3. Assess overall relevance to home improvement (0-100%)");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"lineItemAnalysis\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"Original line item text\",");
            promptBuilder.AppendLine("      \"homeImprovement\": true/false,");
            promptBuilder.AppendLine("      \"category\": \"Product/service category\",");
            promptBuilder.AppendLine("      \"explanation\": \"Your analysis of this item\",");
            promptBuilder.AppendLine("      \"pricingAssessment\": \"Reasonable/High/Low\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"categories\": [\"List\", \"of\", \"all\", \"categories\", \"identified\"],");
            promptBuilder.AppendLine("  \"primaryCategory\": \"Most dominant category\",");
            promptBuilder.AppendLine("  \"homeImprovementRelevance\": 0-100,");
            promptBuilder.AppendLine("  \"overallAssessment\": \"Overall assessment of the invoice's purpose\"");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private Models.Analysis.LineItemAnalysisResult ParseLineItemAnalysisResponse(string responseText, Invoice invoice)
        {
            var result = new Models.Analysis.LineItemAnalysisResult
            {
                Categories = new List<string>(),
                LineItemAnalysis = new List<Models.Analysis.LineItemAnalysisDetails>()
            };

            try
            {
                // Extract JSON part from response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from Gemini line item analysis response");
                    result.PrimaryCategory = "Unknown";
                    result.OverallAssessment = "Failed to parse Gemini response";
                    return result;
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
                
                // Parse line item analysis details
                if (root.TryGetProperty("lineItemAnalysis", out var lineItemsElement) && 
                    lineItemsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in lineItemsElement.EnumerateArray())
                    {
                        // Explicitly use Models.Analysis namespace
                        var analysisItem = new Models.Analysis.LineItemAnalysisDetails();

                        if (item.TryGetProperty("description", out var descElement) &&
                            descElement.ValueKind == JsonValueKind.String)
                        {
                            analysisItem.Description = descElement.GetString() ?? string.Empty;
                        }
                        
                        if (item.TryGetProperty("homeImprovement", out var hiElement))
                        {
                            analysisItem.IsHomeImprovement = hiElement.ValueKind == JsonValueKind.True;
                        }
                        
                        if (item.TryGetProperty("category", out var categoryElement) && 
                            categoryElement.ValueKind == JsonValueKind.String)
                        {
                            analysisItem.Category = categoryElement.GetString() ?? string.Empty;
                        }
                        
                        if (item.TryGetProperty("explanation", out var explanationElement) && 
                            explanationElement.ValueKind == JsonValueKind.String)
                        {
                            analysisItem.Explanation = explanationElement.GetString() ?? string.Empty;
                        }
                        
                        if (item.TryGetProperty("pricingAssessment", out var pricingElement) && 
                            pricingElement.ValueKind == JsonValueKind.String)
                        {
                            analysisItem.PricingAssessment = pricingElement.GetString() ?? "Unknown";
                        }
                        
                        result.LineItemAnalysis.Add(analysisItem);
                    }
                }
                
                // Parse categories
                if (root.TryGetProperty("categories", out var categoriesElement) && 
                    categoriesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var category in categoriesElement.EnumerateArray())
                    {
                        if (category.ValueKind == JsonValueKind.String)
                        {
                            result.Categories.Add(category.GetString() ?? string.Empty);
                        }
                    }
                }
                
                // Parse primary category
                if (root.TryGetProperty("primaryCategory", out var primaryCategoryElement) && 
                    primaryCategoryElement.ValueKind == JsonValueKind.String)
                {
                    result.PrimaryCategory = primaryCategoryElement.GetString() ?? "Unknown";
                }
                else
                {
                    result.PrimaryCategory = result.Categories.Count > 0 ? result.Categories[0] : "Unknown";
                }
                
                // Parse home improvement relevance
                if (root.TryGetProperty("homeImprovementRelevance", out var relevanceElement) && 
                    relevanceElement.ValueKind == JsonValueKind.Number)
                {
                    result.HomeImprovementRelevance = relevanceElement.GetInt32();
                }
                
                // Parse overall assessment
                if (root.TryGetProperty("overallAssessment", out var assessmentElement) && 
                    assessmentElement.ValueKind == JsonValueKind.String)
                {
                    result.OverallAssessment = assessmentElement.GetString() ?? string.Empty;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing line item analysis response");
                result.PrimaryCategory = "Error";
                result.OverallAssessment = $"Error analyzing line items: {ex.Message}";
                return result;
            }
        }

        private string BuildAuditAssessmentPrompt(Invoice invoice)
        {
            var promptBuilder = new System.Text.StringBuilder();
            
            promptBuilder.AppendLine("### AUDIT ASSESSMENT ###");
            promptBuilder.AppendLine("You are a financial auditor specializing in home improvement and construction invoices.");
            promptBuilder.AppendLine("Your task is to provide a detailed audit-ready assessment of this invoice.");
            
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
            
            // Additional context from raw text if available
            if (!string.IsNullOrEmpty(invoice.RawText))
            {
                promptBuilder.AppendLine("\n### EXTRACTED TEXT ###");
                promptBuilder.AppendLine(invoice.RawText.Length > 1000 
                    ? invoice.RawText.Substring(0, 1000) + "..." 
                    : invoice.RawText);
            }
            
            promptBuilder.AppendLine("\n### AUDIT INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Verify the invoice for completeness and accuracy:");
            promptBuilder.AppendLine("   - Check if all required information is present (invoice number, date, vendor details, line items)");
            promptBuilder.AppendLine("   - Assess if amounts and calculations appear correct");
            promptBuilder.AppendLine("   - Identify any inconsistencies or red flags");
            promptBuilder.AppendLine("2. Evaluate compliance with bouwdepot requirements:");
            promptBuilder.AppendLine("   - Determine if the invoice is for legitimate home improvement purposes");
            promptBuilder.AppendLine("   - Assess if the work/products are eligible for bouwdepot financing");
            promptBuilder.AppendLine("3. Provide a detailed audit assessment with:");
            promptBuilder.AppendLine("   - Overall validity determination");
            promptBuilder.AppendLine("   - Specific issues or concerns");
            promptBuilder.AppendLine("   - Recommendations for approval/rejection");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isValid\": true/false,");
            promptBuilder.AppendLine("  \"auditJustification\": \"Detailed explanation of your assessment\",");
            promptBuilder.AppendLine("  \"weightedScore\": 0-100,");
            promptBuilder.AppendLine("  \"regulatoryNotes\": \"Any regulatory or compliance notes\",");
            promptBuilder.AppendLine("  \"issues\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"severity\": \"Error/Warning/Info\",");
            promptBuilder.AppendLine("      \"message\": \"Description of the issue\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"criteriaAssessments\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"criterionName\": \"Name of criterion\",");
            promptBuilder.AppendLine("      \"weight\": 0-1,");
            promptBuilder.AppendLine("      \"evidence\": \"Evidence supporting assessment\",");
            promptBuilder.AppendLine("      \"score\": 0-100,");
            promptBuilder.AppendLine("      \"confidence\": 0-1,");
            promptBuilder.AppendLine("      \"reasoning\": \"Reasoning behind assessment\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"recommendations\": \"Specific recommendations for approval/rejection\"");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }

        private void ParseAuditAssessmentResponse(string responseText, ValidationResult result)
        {
            try
            {
                _logger.LogInformation("Parsing audit assessment response");
                
                // Set default values
                result.IsValid = true;
                
                // Try to extract JSON from the response
                var jsonMatch = Regex.Match(responseText, @"\{.*\}", RegexOptions.Singleline);
                if (jsonMatch.Success)
                {
                    var jsonResponse = jsonMatch.Value;
                    _logger.LogDebug("Extracted JSON from response: {JsonLength} characters", jsonResponse.Length);
                    
                    try
                    {
                        // Use case-insensitive deserialization and allow trailing commas
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            AllowTrailingCommas = true,
                            ReadCommentHandling = JsonCommentHandling.Skip
                        };
                        
                        // Parse using JsonDocument for more flexibility
                        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                        var root = doc.RootElement;
                        
                        // Extract isValid
                        if (root.TryGetProperty("isValid", out var isValidElement))
                        {
                            if (isValidElement.ValueKind == JsonValueKind.True || 
                                isValidElement.ValueKind == JsonValueKind.False)
                            {
                                result.IsValid = isValidElement.GetBoolean();
                            }
                        }
                        
                        // Extract auditJustification
                        if (root.TryGetProperty("auditJustification", out var justificationElement) && 
                            justificationElement.ValueKind == JsonValueKind.String)
                        {
                            result.AuditJustification = justificationElement.GetString();
                            _logger.LogDebug("Extracted audit justification: {Length} characters", 
                                result.AuditJustification?.Length ?? 0);
                        }
                        
                        // Extract weightedScore
                        if (root.TryGetProperty("weightedScore", out var scoreElement) && 
                            scoreElement.ValueKind == JsonValueKind.Number)
                        {
                            result.WeightedScore = (int)scoreElement.GetDouble();
                            _logger.LogDebug("Extracted weighted score: {Score}", result.WeightedScore);
                        }
                        
                        // Extract regulatoryNotes
                        if (root.TryGetProperty("regulatoryNotes", out var notesElement) && 
                            notesElement.ValueKind == JsonValueKind.String)
                        {
                            string notes = notesElement.GetString() ?? string.Empty;
                            result.RegulatoryNotes = new List<string> { notes };
                            _logger.LogDebug("Extracted regulatory notes: {Length} characters", 
                                notes.Length);
                        }
                        
                        // Extract issues
                        if (root.TryGetProperty("issues", out var issuesElement) && 
                            issuesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var issue in issuesElement.EnumerateArray())
                            {
                                string message = string.Empty;
                                ValidationSeverity severity = ValidationSeverity.Warning;
                                
                                if (issue.TryGetProperty("message", out var messageElement) && 
                                    messageElement.ValueKind == JsonValueKind.String)
                                {
                                    message = messageElement.GetString() ?? string.Empty;
                                }
                                
                                if (issue.TryGetProperty("severity", out var severityElement) && 
                                    severityElement.ValueKind == JsonValueKind.String)
                                {
                                    string severityStr = severityElement.GetString()?.ToLower() ?? "warning";
                                    
                                    if (severityStr == "error")
                                    {
                                        severity = ValidationSeverity.Error;
                                    }
                                    else if (severityStr == "info")
                                    {
                                        severity = ValidationSeverity.Info;
                                    }
                                }
                                
                                if (!string.IsNullOrEmpty(message))
                                {
                                    result.AddIssue(severity, message);
                                }
                            }
                        }
                        
                        // Extract criteriaAssessments
                        if (root.TryGetProperty("criteriaAssessments", out var criteriaElement) && 
                            criteriaElement.ValueKind == JsonValueKind.Array)
                        {
                            result.CriteriaAssessments = new List<CriterionAssessment>();
                            
                            foreach (var criterion in criteriaElement.EnumerateArray())
                            {
                                var assessment = new CriterionAssessment();
                                
                                if (criterion.TryGetProperty("criterionName", out var nameElement) && 
                                    nameElement.ValueKind == JsonValueKind.String)
                                {
                                    assessment.CriterionName = nameElement.GetString() ?? string.Empty;
                                }
                                
                                if (criterion.TryGetProperty("weight", out var weightElement) && 
                                    weightElement.ValueKind == JsonValueKind.Number)
                                {
                                    assessment.Weight = Convert.ToDecimal(weightElement.GetDouble());
                                }
                                
                                if (criterion.TryGetProperty("evidence", out var evidenceElement) && 
                                    evidenceElement.ValueKind == JsonValueKind.String)
                                {
                                    assessment.Evidence = evidenceElement.GetString() ?? string.Empty;
                                }
                                
                                if (criterion.TryGetProperty("score", out var criterionScoreElement) && 
                                    criterionScoreElement.ValueKind == JsonValueKind.Number)
                                {
                                    assessment.Score = (int)criterionScoreElement.GetDouble();
                                }
                                
                                if (criterion.TryGetProperty("confidence", out var confidenceElement) && 
                                    confidenceElement.ValueKind == JsonValueKind.Number)
                                {
                                    assessment.Confidence = (int)confidenceElement.GetDouble();
                                }
                                
                                if (criterion.TryGetProperty("reasoning", out var reasoningElement) && 
                                    reasoningElement.ValueKind == JsonValueKind.String)
                                {
                                    assessment.Reasoning = reasoningElement.GetString() ?? string.Empty;
                                }
                                
                                result.CriteriaAssessments.Add(assessment);
                            }
                            
                            _logger.LogDebug("Extracted {Count} criteria assessments", 
                                result.CriteriaAssessments.Count);
                        }
                        
                        // Extract recommendations
                        if (root.TryGetProperty("recommendations", out var recommendationsElement) && 
                            recommendationsElement.ValueKind == JsonValueKind.String)
                        {
                            result.AuditRecommendation = recommendationsElement.GetString();
                            _logger.LogDebug("Extracted audit recommendation: {Length} characters", 
                                result.AuditRecommendation?.Length ?? 0);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Error parsing JSON from audit assessment response");
                        // Fall back to regex-based parsing
                        FallbackToRegexParsing(responseText, result);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not extract JSON from response, falling back to regex parsing");
                    // Fall back to regex-based parsing
                    FallbackToRegexParsing(responseText, result);
                }
                
                // If no issues were found but the result is invalid, add a generic issue
                if (result.Issues.Count == 0 && !result.IsValid)
                {
                    result.AddIssue(ValidationSeverity.Error, 
                        "The invoice does not meet bouwdepot requirements based on AI assessment.");
                }
                
                _logger.LogInformation("Completed parsing audit assessment: IsValid={IsValid}, IssueCount={IssueCount}", 
                    result.IsValid, result.Issues.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing audit assessment response");
                result.IsValid = false;
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error processing audit assessment: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Fallback method to parse the response using regex when JSON parsing fails
        /// </summary>
        private void FallbackToRegexParsing(string responseText, ValidationResult result)
        {
            _logger.LogInformation("Using regex fallback parsing for audit assessment");
            
            // Extract validity
            var validityMatch = Regex.Match(responseText, @"(?i)valid(?:ity)?:?\s*(yes|no|valid|invalid|true|false)");
            if (validityMatch.Success)
            {
                var validityValue = validityMatch.Groups[1].Value.ToLower();
                result.IsValid = validityValue == "yes" || validityValue == "valid" || validityValue == "true";
            }
            
            // Extract audit justification
            var justificationMatch = Regex.Match(responseText, @"(?i)(?:justification|assessment|analysis):(.*?)(?:\n\n|\n#|\Z)", RegexOptions.Singleline);
            if (justificationMatch.Success)
            {
                result.AuditJustification = justificationMatch.Groups[1].Value.Trim();
            }
            
            // Extract weighted score
            var scoreMatch = Regex.Match(responseText, @"(?i)(?:score|rating):\s*(\d+)");
            if (scoreMatch.Success && double.TryParse(scoreMatch.Groups[1].Value, out double score))
            {
                result.WeightedScore = (int)score;
            }
            
            // Extract regulatory notes
            var notesMatch = Regex.Match(responseText, @"(?i)(?:regulatory|compliance|legal)(?:\s+notes?|requirements?|considerations?):(.*?)(?:\n\n|\n#|\Z)", RegexOptions.Singleline);
            if (notesMatch.Success)
            {
                string notes = notesMatch.Groups[1].Value.Trim();
                result.RegulatoryNotes = new List<string> { notes };
            }
            
            // Look for issues section
            var issuesMatch = Regex.Match(responseText, @"(?i)(?:issues|concerns|problems):(.*?)(?:\n\n|\n#|\Z)", RegexOptions.Singleline);
            if (issuesMatch.Success)
            {
                var issuesText = issuesMatch.Groups[1].Value.Trim();
                var issueLines = issuesText.Split('\n');
                
                foreach (var line in issueLines)
                {
                    var trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                    {
                        // Determine severity based on keywords
                        var severity = ValidationSeverity.Warning;
                        if (trimmedLine.Contains("critical") || trimmedLine.Contains("severe") || 
                            trimmedLine.Contains("major") || trimmedLine.Contains("reject"))
                        {
                            severity = ValidationSeverity.Error;
                        }
                        else if (trimmedLine.Contains("minor") || trimmedLine.Contains("suggestion") ||
                                 trimmedLine.Contains("note"))
                        {
                            severity = ValidationSeverity.Info;
                        }
                        
                        result.AddIssue(severity, trimmedLine);
                    }
                }
            }
            
            // Look for recommendations section
            var recommendationsMatch = Regex.Match(responseText, @"(?i)(?:recommendations?|conclusion):(.*?)(?:\n\n|\n#|\Z)", RegexOptions.Singleline);
            if (recommendationsMatch.Success)
            {
                var recommendationsText = recommendationsMatch.Groups[1].Value.Trim();
                result.AuditRecommendation = recommendationsText;
            }
        }
        
        #endregion
    }
}
