using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Provides detailed explanations of AI decisions for assurance and transparency
    /// </summary>
    public class AIDecisionExplainer : IAIDecisionExplainer
    {
        private readonly ILogger<AIDecisionExplainer> _logger;
        private readonly IGeminiService _geminiService;
        
        public AIDecisionExplainer(
            ILogger<AIDecisionExplainer> logger,
            IGeminiService geminiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
        }
        
        /// <inheritdoc />
        public async Task<AIDecisionExplanation> ExplainValidationResultAsync(ValidationResult validationResult)
        {
            _logger.LogInformation("Generating explanation for validation result: {InvoiceNumber}", 
                validationResult.ExtractedInvoice?.InvoiceNumber ?? "Unknown");
            
            try
            {
                // Create base explanation
                var explanation = new AIDecisionExplanation
                {
                    SummaryExplanation = "Analysis in progress...",
                    DetailedExplanation = "Processing detailed explanation...",
                    TechnicalExplanation = "Technical analysis in preparation..."
                };
                
                // Build prompt for Gemini to explain the validation result
                string prompt = BuildExplanationPrompt(validationResult);
                
                // Call Gemini API to get detailed explanation
                string response = await _geminiService.GetConversationPromptAsync(prompt);
                
                // Parse the response to extract explanations
                var parsedExplanation = ParseExplanationResponse(response, validationResult);
                
                _logger.LogInformation("Successfully generated explanation for validation result");
                
                return parsedExplanation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating explanation for validation result");
                
                // Return a basic explanation with error information
                return new AIDecisionExplanation
                {
                    SummaryExplanation = "An error occurred while generating the explanation.",
                    DetailedExplanation = $"Error: {ex.Message}",
                    TechnicalExplanation = $"Exception: {ex.GetType().Name}, Stack: {ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace?.Length ?? 0, 200))}"
                };
            }
        }
        
        /// <inheritdoc />
        public async Task<AIDecisionExplanation> ExplainVendorVerificationAsync(string vendorName, VendorVerificationResult verification)
        {
            _logger.LogInformation("Generating explanation for vendor verification: {VendorName}", vendorName);
            
            try
            {
                // Build prompt for Gemini to explain the vendor verification
                string prompt = BuildVendorVerificationPrompt(vendorName, verification);
                
                // Call Gemini API to get detailed explanation
                string response = await _geminiService.GetConversationPromptAsync(prompt);
                
                // Parse the response to extract explanations
                var explanation = ParseVendorVerificationResponse(response, verification);
                
                _logger.LogInformation("Successfully generated vendor verification explanation");
                
                return explanation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating vendor verification explanation");
                
                // Return a basic explanation with error information
                return new AIDecisionExplanation
                {
                    SummaryExplanation = "An error occurred while generating the vendor verification explanation.",
                    DetailedExplanation = $"Error: {ex.Message}",
                    TechnicalExplanation = ex.ToString()
                };
            }
        }
        
        /// <inheritdoc />
        public async Task<AIDecisionExplanation> ExplainValidationScoreAsync(string scoreType, int score, List<string> factors)
        {
            _logger.LogInformation("Generating explanation for validation score: {ScoreType}", scoreType);
            
            try
            {
                // Build prompt for Gemini to explain the score
                string prompt = BuildScoreExplanationPrompt(scoreType, score, factors);
                
                // Call Gemini API to get detailed explanation
                string response = await _geminiService.GetConversationPromptAsync(prompt);
                
                // Parse the response to extract explanations
                var explanation = ParseScoreExplanationResponse(response, scoreType, score, factors);
                
                _logger.LogInformation("Successfully generated score explanation for {ScoreType}", scoreType);
                
                return explanation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating score explanation");
                
                // Return a basic explanation with error information
                return new AIDecisionExplanation
                {
                    SummaryExplanation = $"The {scoreType} score is {score}/100.",
                    DetailedExplanation = $"Error generating detailed explanation: {ex.Message}",
                    Factors = factors.Select(f => new DecisionFactor { Name = f, Impact = "Unknown" }).ToList()
                };
            }
        }
        
        /// <inheritdoc />
        public async Task<ConfidenceAnalysis> AnalyzeConfidenceAsync(ValidationResult validationResult)
        {
            _logger.LogInformation("Analyzing confidence for validation result: {InvoiceNumber}", 
                validationResult.ExtractedInvoice?.InvoiceNumber ?? "Unknown");
            
            try
            {
                // Build prompt for Gemini to analyze confidence
                string prompt = BuildConfidenceAnalysisPrompt(validationResult);
                
                // Call Gemini API to get detailed analysis
                string response = await _geminiService.GetConversationPromptAsync(prompt);
                
                // Parse the response to extract confidence analysis
                var analysis = ParseConfidenceAnalysisResponse(response, validationResult);
                
                _logger.LogInformation("Successfully analyzed confidence for validation result");
                
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing confidence");
                
                // Return a basic analysis with error information
                return new ConfidenceAnalysis
                {
                    OverallConfidence = validationResult.ConfidenceScore,
                    DistributionType = "Unknown",
                    ConfidenceMethodology = $"Error: {ex.Message}",
                    ConfidenceDecreasingFactors = new List<string> { "Error in confidence analysis" }
                };
            }
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Builds a prompt for explaining a validation result
        /// </summary>
        private string BuildExplanationPrompt(ValidationResult validationResult)
        {
            // Extract key information from the validation result
            string validityState = validationResult.IsValid ? "valid" : "invalid";
            string homeImprovementState = validationResult.IsHomeImprovement ? "is for home improvement" : "is not for home improvement";
            string complianceState = validationResult.IsBouwdepotCompliant
                ? "complies with Bouwdepot rules" 
                : "does not comply with Bouwdepot rules";
            
            return $@"
### DECISION EXPLANATION REQUEST ###

I need you to provide a clear, detailed explanation of the AI's decision about an invoice validation.
Explain the decision as if you were writing for an auditor or compliance officer who needs to understand exactly why the AI reached its conclusion.

## VALIDATION RESULT ##
- Invoice validity: {validityState} (confidence: {validationResult.ConfidenceScore}%)
- Home improvement status: {homeImprovementState}
- Bouwdepot compliance: {complianceState}

## INVOICE DETAILS ##
- Invoice number: {validationResult.ExtractedInvoice?.InvoiceNumber}
- Vendor: {validationResult.ExtractedInvoice?.VendorName}
- Total amount: {validationResult.ExtractedInvoice?.TotalAmount}
- Date: {validationResult.ExtractedInvoice?.InvoiceDate}

## LINE ITEMS ##
{string.Join("\n", validationResult.ExtractedInvoice?.LineItems.Select(item => $"- {item.Description}: {item.TotalPrice}") ?? Array.Empty<string>())}

## VALIDATION NOTES ##
{string.Join("\n", validationResult.Issues.Select(i => $"- {i.Severity}: {i.Message}"))}

## AUDIT NOTES ##
{validationResult.AuditJustification ?? "No audit justification provided"}
{(validationResult.AuditReport?.ExecutiveSummary != null ? $"Audit Summary: {validationResult.AuditReport.ExecutiveSummary}" : "")}

## WHAT I NEED ##
1. A short summary explanation (2-3 sentences) for a busy manager
2. A detailed explanation showing the full reasoning chain (step by step)
3. A technical explanation including model details and confidence analysis
4. The key decision factors with their relative weights/importance
5. The specific evidence used to make this decision
6. Any alternate interpretations that were considered but rejected
7. References to specific Bouwdepot rules that applied
8. Potential weak points or limitations in this analysis

Respond with a JSON object that has the following structure:
{{
  ""summaryExplanation"": ""..."",
  ""detailedExplanation"": ""..."",
  ""technicalExplanation"": ""..."",
  ""factors"": [
    {{
      ""name"": ""Factor name"",
      ""weight"": 75,
      ""impact"": ""How this factor influenced the decision"",
      ""value"": ""Assessment of this factor""
    }}
  ],
  ""evidence"": [
    {{
      ""type"": ""Type of evidence"",
      ""location"": ""Where in the document"",
      ""content"": ""The actual evidence"",
      ""relevance"": 85
    }}
  ],
  ""alternateInterpretations"": [
    {{
      ""description"": ""The alternate interpretation"",
      ""confidenceScore"": 20,
      ""rejectionReasons"": [""Reason 1"", ""Reason 2""]
    }}
  ],
  ""applicableRules"": [
    {{
      ""ruleId"": ""BD-001"",
      ""ruleName"": ""Permanent Attachment Rule"",
      ""description"": ""Items must be permanently attached to the property"",
      ""isSatisfied"": true,
      ""evidence"": ""Evidence for this rule assessment""
    }}
  ],
  ""potentialWeakPoints"": [""Weak point 1"", ""Weak point 2""],
  ""confidence"": {{
    ""overallScore"": 82,
    ""marginOfError"": 5.2,
    ""approvalThreshold"": 70,
    ""confidenceLevel"": 0.95
  }}
}}
";
        }
        
        /// <summary>
        /// Builds a prompt for explaining vendor verification
        /// </summary>
        private string BuildVendorVerificationPrompt(string vendorName, VendorVerificationResult verification)
        {
            return $@"
### VENDOR VERIFICATION EXPLANATION REQUEST ###

I need you to provide a clear, detailed explanation of the AI's verification assessment of a vendor.
Explain the verification as if you were writing for an auditor or compliance officer who needs to understand exactly why the AI reached its conclusion.

## VERIFICATION RESULT ##
- Vendor name: {vendorName}
- Verification status: {(verification.IsVerified ? "Verified" : "Not verified")}
- Confidence score: {verification.ConfidenceScore}%
- Risk level: {verification.RiskProfile.RiskLevel}
- Risk score: {verification.RiskProfile.RiskScore}/100

## VERIFICATION SOURCES ##
{string.Join("\n", verification.VerificationSources.Select(s => $"- {s}"))}

## DETECTED ANOMALIES ##
{string.Join("\n", verification.DetectedAnomalies.Select(a => $"- {a}"))}

## RISK FACTORS ##
{string.Join("\n", verification.RiskProfile.RiskFactors.Select(r => $"- {r}"))}

## MITIGATING FACTORS ##
{string.Join("\n", verification.RiskProfile.MitigatingFactors.Select(m => $"- {m}"))}

## WHAT I NEED ##
1. A short summary explanation (2-3 sentences) for a busy manager
2. A detailed explanation showing the full reasoning chain
3. A technical explanation including verification methodology
4. The key decision factors with their relative weights/importance
5. The specific evidence used to make this verification decision
6. Any alternate interpretations that were considered but rejected
7. Potential weak points or limitations in this analysis

Respond with a JSON object with the same structure as requested in the validation explanation prompt.
";
        }
        
        /// <summary>
        /// Builds a prompt for explaining a validation score
        /// </summary>
        private string BuildScoreExplanationPrompt(string scoreType, int score, List<string> factors)
        {
            return $@"
### VALIDATION SCORE EXPLANATION REQUEST ###

I need you to provide a clear, detailed explanation of how a specific validation score was calculated.
Explain the score calculation as if you were writing for an auditor or compliance officer who needs to understand exactly how the AI reached this number.

## SCORE DETAILS ##
- Score type: {scoreType}
- Score value: {score}/100
- Contributing factors:
{string.Join("\n", factors.Select(f => $"- {f}"))}

## WHAT I NEED ##
1. A short summary explanation (2-3 sentences) for a busy manager
2. A detailed explanation showing how each factor contributed to the score
3. A technical explanation of the scoring algorithm and weightings
4. The key decision factors with their relative weights/importance

Respond with a JSON object with the same structure as requested in the validation explanation prompt.
";
        }
        
        /// <summary>
        /// Builds a prompt for analyzing confidence
        /// </summary>
        private string BuildConfidenceAnalysisPrompt(ValidationResult validationResult)
        {
            return $@"
### CONFIDENCE ANALYSIS REQUEST ###

I need you to provide a detailed analysis of the confidence level in an invoice validation decision.
Explain the confidence assessment as if you were writing for an auditor or risk officer who needs to understand the reliability of the AI's conclusion.

## VALIDATION RESULT ##
- Invoice validity: {(validationResult.IsValid ? "valid" : "invalid")}
- Confidence score: {validationResult.ConfidenceScore}%
- Home improvement status: {(validationResult.IsHomeImprovement ? "is for home improvement" : "is not for home improvement")}
- Bouwdepot compliance: {(validationResult.IsBouwdepotCompliant ? "complies with Bouwdepot rules" : "does not comply with Bouwdepot rules")}

## CONFIDENCE FACTORS ##
{string.Join("\n", validationResult.ConfidenceFactors.Select(f => $"- {f.FactorName}: Impact = {f.Impact}, Explanation = {f.Explanation}"))}

## WHAT I NEED ##
1. The overall confidence level
2. Confidence intervals for different aspects of the validation
3. The statistical distribution type that best represents the confidence
4. The methodology used to calculate confidence
5. Factors that could increase confidence if available
6. Factors that decrease confidence

Respond with a JSON object with the following structure:
{{
  ""overallConfidence"": 85,
  ""confidenceIntervals"": {{
    ""homeImprovement"": {{
      ""lowerBound"": 75.2,
      ""upperBound"": 92.3,
      ""confidenceLevel"": 0.95
    }},
    ""validInvoice"": {{
      ""lowerBound"": 80.1,
      ""upperBound"": 95.4,
      ""confidenceLevel"": 0.95
    }}
  }},
  ""distributionType"": ""Normal distribution"",
  ""confidenceMethodology"": ""Explanation of methodology..."",
  ""confidenceIncreasingFactors"": [""Factor 1"", ""Factor 2""],
  ""confidenceDecreasingFactors"": [""Factor 1"", ""Factor 2""]
}}
";
        }
        
        /// <summary>
        /// Parses the explanation response from Gemini
        /// </summary>
        private AIDecisionExplanation ParseExplanationResponse(string response, ValidationResult validationResult)
        {
            try
            {
                // Try to extract and parse JSON from the response
                var jsonResponse = ExtractJsonFromResponse(response);
                var explanation = JsonSerializer.Deserialize<AIDecisionExplanation>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // If parsing succeeded, return the explanation
                if (explanation != null)
                {
                    return explanation;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse explanation response as JSON, using fallback parsing");
            }
            
            // Fallback to simple text extraction if JSON parsing fails
            return FallbackParseExplanation(response, validationResult);
        }
        
        /// <summary>
        /// Fallback parsing for explanation responses
        /// </summary>
        private AIDecisionExplanation FallbackParseExplanation(string response, ValidationResult validationResult)
        {
            // Extract sections based on headings
            var explanation = new AIDecisionExplanation();
            
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            // Simple approach to extract sections
            var summaryLines = new List<string>();
            var detailedLines = new List<string>();
            var technicalLines = new List<string>();
            
            string currentSection = "";
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.Contains("SUMMARY", StringComparison.OrdinalIgnoreCase) || 
                    trimmedLine.StartsWith("Summary:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "summary";
                    continue;
                }
                else if (trimmedLine.Contains("DETAILED", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Detailed explanation:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "detailed";
                    continue;
                }
                else if (trimmedLine.Contains("TECHNICAL", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Technical:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "technical";
                    continue;
                }
                else if (trimmedLine.Contains("FACTORS", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Factors:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "factors";
                    continue;
                }
                else if (trimmedLine.Contains("EVIDENCE", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Evidence:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "evidence";
                    continue;
                }
                else if (trimmedLine.Contains("ALTERNATE", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Alternate interpretations:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "alternate";
                    continue;
                }
                else if (trimmedLine.Contains("RULES", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Applicable rules:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "rules";
                    continue;
                }
                else if (trimmedLine.Contains("WEAK", StringComparison.OrdinalIgnoreCase) || 
                         trimmedLine.StartsWith("Weak points:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "weak";
                    continue;
                }
                
                // Add line to appropriate section
                switch (currentSection)
                {
                    case "summary":
                        summaryLines.Add(trimmedLine);
                        break;
                    case "detailed":
                        detailedLines.Add(trimmedLine);
                        break;
                    case "technical":
                        technicalLines.Add(trimmedLine);
                        break;
                    case "factors":
                        if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                        {
                            explanation.Factors.Add(new DecisionFactor { 
                                Name = trimmedLine.Substring(2), 
                                Impact = "Unknown impact" 
                            });
                        }
                        break;
                    case "evidence":
                        if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                        {
                            explanation.Evidence.Add(new EvidenceItem { 
                                Type = "Text", 
                                Content = trimmedLine.Substring(2) 
                            });
                        }
                        break;
                    case "alternate":
                        if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                        {
                            explanation.AlternateInterpretations.Add(new AlternateInterpretation { 
                                Description = trimmedLine.Substring(2)
                            });
                        }
                        break;
                    case "rules":
                        if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                        {
                            explanation.ApplicableRules.Add(new RuleReference { 
                                RuleName = trimmedLine.Substring(2)
                            });
                        }
                        break;
                    case "weak":
                        if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                        {
                            explanation.PotentialWeakPoints.Add(trimmedLine.Substring(2));
                        }
                        break;
                }
            }
            
            // Join the lines for each section
            explanation.SummaryExplanation = string.Join(" ", summaryLines);
            explanation.DetailedExplanation = string.Join("\n", detailedLines);
            explanation.TechnicalExplanation = string.Join("\n", technicalLines);
            
            // If we couldn't extract anything, provide basic fallbacks
            if (string.IsNullOrWhiteSpace(explanation.SummaryExplanation))
            {
                explanation.SummaryExplanation = $"The invoice was found to be {(validationResult.IsValid ? "valid" : "invalid")} " +
                                              $"with a confidence of {validationResult.ConfidenceScore}%. " +
                                              $"It {(validationResult.IsHomeImprovement ? "is" : "is not")} for home improvement purposes.";
            }
            
            if (string.IsNullOrWhiteSpace(explanation.DetailedExplanation))
            {
                explanation.DetailedExplanation = $"The detailed analysis found that this invoice " +
                                               $"{(validationResult.IsBouwdepotCompliant ? "complies with" : "does not comply with")} " +
                                               $"Bouwdepot rules based on the extracted data and validation checks.";
            }
            
            // Add default confidence metrics
            explanation.Confidence = new ConfidenceMetrics
            {
                OverallScore = validationResult.ConfidenceScore,
                MarginOfError = 5.0,
                ApprovalThreshold = 70,
                ConfidenceLevel = 0.95
            };
            
            return explanation;
        }
        
        /// <summary>
        /// Parses a vendor verification response
        /// </summary>
        private AIDecisionExplanation ParseVendorVerificationResponse(string response, VendorVerificationResult verification)
        {
            // Similar approach to ParseExplanationResponse
            try
            {
                // Try to extract and parse JSON from the response
                var jsonResponse = ExtractJsonFromResponse(response);
                var explanation = JsonSerializer.Deserialize<AIDecisionExplanation>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // If parsing succeeded, return the explanation
                if (explanation != null)
                {
                    return explanation;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse vendor verification response as JSON, using fallback parsing");
            }
            
            // Fallback to simple text extraction
            var fallbackExplanation = new AIDecisionExplanation
            {
                SummaryExplanation = $"The vendor verification analysis found that the vendor is {(verification.IsVerified ? "verified" : "not verified")} " +
                                 $"with a confidence of {verification.ConfidenceScore}%. Risk level: {verification.RiskProfile.RiskLevel}.",
                DetailedExplanation = "The vendor verification process analyzed multiple data sources to determine the authenticity and risk profile of the vendor.",
                TechnicalExplanation = $"Verification used {verification.VerificationSources.Count} sources with a weighted confidence score of {verification.ConfidenceScore}%."
            };
            
            // Add factors from risk profile
            foreach (var factor in verification.RiskProfile.RiskFactors)
            {
                fallbackExplanation.Factors.Add(new DecisionFactor
                {
                    Name = factor,
                    Impact = "Negative impact on verification",
                    Weight = 50
                });
            }
            
            foreach (var factor in verification.RiskProfile.MitigatingFactors)
            {
                fallbackExplanation.Factors.Add(new DecisionFactor
                {
                    Name = factor,
                    Impact = "Positive impact on verification",
                    Weight = 40
                });
            }
            
            // Add sources as evidence
            foreach (var source in verification.VerificationSources)
            {
                fallbackExplanation.Evidence.Add(new EvidenceItem
                {
                    Type = "Verification Source",
                    Content = source,
                    Relevance = 80
                });
            }
            
            // Add anomalies as potential weak points
            fallbackExplanation.PotentialWeakPoints.AddRange(verification.DetectedAnomalies);
            
            return fallbackExplanation;
        }
        
        /// <summary>
        /// Parses a score explanation response
        /// </summary>
        private AIDecisionExplanation ParseScoreExplanationResponse(string response, string scoreType, int score, List<string> factors)
        {
            // Similar approach to ParseExplanationResponse
            try
            {
                // Try to extract and parse JSON from the response
                var jsonResponse = ExtractJsonFromResponse(response);
                var explanation = JsonSerializer.Deserialize<AIDecisionExplanation>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // If parsing succeeded, return the explanation
                if (explanation != null)
                {
                    return explanation;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse score explanation response as JSON, using fallback parsing");
            }
            
            // Fallback to simple text extraction
            var fallbackExplanation = new AIDecisionExplanation
            {
                SummaryExplanation = $"The {scoreType} score of {score}/100 represents the level of confidence in the assessment.",
                DetailedExplanation = $"The {scoreType} score was calculated based on {factors.Count} factors, resulting in a value of {score}/100."
            };
            
            // Add factors
            int weight = 100 / Math.Max(1, factors.Count);
            foreach (var factor in factors)
            {
                fallbackExplanation.Factors.Add(new DecisionFactor
                {
                    Name = factor,
                    Weight = weight,
                    Impact = "Contributes to overall score"
                });
            }
            
            return fallbackExplanation;
        }
        
        /// <summary>
        /// Parses a confidence analysis response
        /// </summary>
        private ConfidenceAnalysis ParseConfidenceAnalysisResponse(string response, ValidationResult validationResult)
        {
            try
            {
                // Try to extract and parse JSON from the response
                var jsonResponse = ExtractJsonFromResponse(response);
                var analysis = JsonSerializer.Deserialize<ConfidenceAnalysis>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // If parsing succeeded, return the analysis
                if (analysis != null)
                {
                    return analysis;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse confidence analysis response as JSON, using fallback parsing");
            }
            
            // Fallback to simple extraction
            var fallbackAnalysis = new ConfidenceAnalysis
            {
                OverallConfidence = validationResult.ConfidenceScore,
                DistributionType = "Normal distribution",
                ConfidenceMethodology = "Based on weighted average of confidence factors from the validation process."
            };
            
            // Add confidence intervals
            fallbackAnalysis.ConfidenceIntervals["overall"] = new ConfidenceInterval
            {
                LowerBound = Math.Max(0, validationResult.ConfidenceScore - 10),
                UpperBound = Math.Min(100, validationResult.ConfidenceScore + 10),
                ConfidenceLevel = 0.95
            };
            
            // Extract increasing/decreasing factors from validation result
            foreach (var factor in validationResult.ConfidenceFactors)
            {
                if (factor.Impact > 0)
                {
                    fallbackAnalysis.ConfidenceIncreasingFactors.Add(factor.FactorName);
                }
                else if (factor.Impact < 0)
                {
                    fallbackAnalysis.ConfidenceDecreasingFactors.Add(factor.FactorName);
                }
            }
            
            return fallbackAnalysis;
        }
        
        /// <summary>
        /// Extracts JSON from a response text
        /// </summary>
        private string ExtractJsonFromResponse(string response)
        {
            // Find JSON object in response
            int startIndex = response.IndexOf('{');
            int endIndex = response.LastIndexOf('}');
            
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return response.Substring(startIndex, endIndex - startIndex + 1);
            }
            
            // JSON not found, throw exception to trigger fallback parsing
            throw new FormatException("JSON not found in response");
        }
        
        #endregion
    }
}
