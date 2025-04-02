using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for advanced invoice analysis using Gemini API (multi-modal and audit functionality)
    /// </summary>
    public class GeminiAdvancedAnalysisService : GeminiServiceBase
    {
        private readonly ILogger<GeminiAdvancedAnalysisService> _advancedLogger;
        
        public GeminiAdvancedAnalysisService(ILogger<GeminiAdvancedAnalysisService> logger, IConfiguration configuration) 
            : base(logger, configuration)
        {
            _advancedLogger = logger;
        }
        
        /// <summary>
        /// Extracts invoice data from images using Gemini AI
        /// </summary>
        public async Task<Invoice> ExtractInvoiceDataFromImagesAsync(Invoice invoice)
        {
            _advancedLogger.LogInformation("Extracting invoice data from images using Gemini AI: {FileName}", invoice.FileName);
            
            try
            {
                // Make sure we have page images
                if (invoice.PageImages == null || invoice.PageImages.Count == 0)
                {
                    _advancedLogger.LogWarning("Invoice data extraction failed: No page images available for {FileName}", invoice.FileName);
                    return invoice;
                }
                
                // Log the number of page images available
                _advancedLogger.LogInformation("Processing invoice data extraction with {PageCount} page images", invoice.PageImages.Count);
                
                // Prepare prompt for data extraction
                var prompt = BuildInvoiceDataExtractionPrompt();
                
                // Call Gemini API with images
                _advancedLogger.LogInformation("Calling Gemini Vision API for invoice data extraction with {Count} images", invoice.PageImages.Count);
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "InvoiceDataExtraction");
                
                // Parse the response and update the invoice
                ParseInvoiceDataExtractionResponse(response, invoice);
                
                _advancedLogger.LogInformation("Successfully extracted invoice data using Gemini AI: " +
                                              "InvoiceNumber={InvoiceNumber}, InvoiceDate={InvoiceDate}, TotalAmount={TotalAmount}", 
                                              invoice.InvoiceNumber, invoice.InvoiceDate, invoice.TotalAmount);
                
                return invoice;
            }
            catch (Exception ex)
            {
                _advancedLogger.LogError(ex, "Error extracting invoice data from images with Gemini for {FileName}", invoice.FileName);
                // Return the original invoice without modifications in case of error
                return invoice;
            }
        }
        
        /// <summary>
        /// Performs visual-only analysis of an invoice using only images
        /// </summary>
        public async Task<ValidationResult> ValidateWithMultiModalAnalysisAsync(Invoice invoice)
        {
            _advancedLogger.LogInformation("Performing visual-only validation of invoice with AI: {FileName}", invoice.FileName);
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Make sure we have page images
                if (invoice.PageImages == null || invoice.PageImages.Count == 0)
                {
                    _advancedLogger.LogWarning("Visual analysis requested but no page images are available: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Visual analysis was not possible because no page images were available.");
                    
                    // Return a result indicating the failure
                    result.IsValid = false;
                    return result;
                }
                
                // Log the number of page images available
                _advancedLogger.LogInformation("Processing visual-only analysis with {PageCount} page images", invoice.PageImages.Count);
                
                // Prepare visual-only prompt for image analysis
                var prompt = BuildVisualOnlyPrompt(invoice);
                
                // Call Gemini API with images and specific operation name for better logs
                _advancedLogger.LogInformation("Calling Gemini Vision API with {Count} images", invoice.PageImages.Count);
                var response = await CallGeminiApiAsync(prompt, invoice.PageImages, "VisualOnlyAnalysis");
                result.RawGeminiResponse = response;
                
                // Parse the response
                ParseMultiModalResponse(response, result);
                
                _advancedLogger.LogInformation("Visual-only validation completed: IsValid={IsValid}, IsHomeImprovement={IsHomeImprovement}", 
                    result.IsValid, result.IsHomeImprovement);
                
                return result;
            }
            catch (Exception ex)
            {
                _advancedLogger.LogError(ex, "Error in visual-only validation with Gemini for {FileName}", invoice.FileName);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating with visual-only analysis: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }
        
        /// <summary>
        /// Generates an audit-ready assessment of an invoice
        /// </summary>
        public async Task<ValidationResult> GetAuditReadyAssessmentAsync(Invoice invoice)
        {
            _advancedLogger.LogInformation("Generating audit-ready assessment for invoice: {FileName}", invoice.FileName);
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Prepare the audit-friendly prompt
                var prompt = BuildAuditReadyPrompt(invoice);
                
                // Call Gemini API with specific operation name for better logs
                var response = await CallGeminiApiAsync(prompt, null, "AuditAssessment");
                result.RawGeminiResponse = response;
                
                // Parse the audit-ready response
                ParseAuditReadyResponse(response, result);
                
                _advancedLogger.LogInformation("Audit-ready assessment completed: IsValid={IsValid}, WeightedScore={WeightedScore}", 
                    result.IsValid, result.WeightedScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _advancedLogger.LogError(ex, "Error generating audit-ready assessment with Gemini for {FileName}", invoice.FileName);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error generating audit assessment: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }
        
        private string BuildVisualOnlyPrompt(Invoice invoice)
        {
            _advancedLogger.LogDebug("Building visual-only analysis prompt for {FileName}", invoice.FileName);
            
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### VISUAL-ONLY INVOICE ANALYSIS ###");
            promptBuilder.AppendLine($"Analysis Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            promptBuilder.AppendLine("Validator Version: Gemini Pro Vision with Visual-Only Analysis v1.0");
            promptBuilder.AppendLine("Analysis Framework: Dutch Construction Invoice Visual Assessment 2025");
            
            promptBuilder.AppendLine("\n### TASK: PURE VISUAL INVOICE VALIDATION ###");
            promptBuilder.AppendLine("You are a specialized invoice validator for Dutch construction and home improvement expenses.");
            promptBuilder.AppendLine("I'm attaching images of an invoice document. Your task is to:");
            promptBuilder.AppendLine("1. ONLY analyze the VISUAL ELEMENTS of this invoice to determine if it's related to home improvement.");
            promptBuilder.AppendLine("2. DO NOT attempt to read or analyze the text content directly.");
            promptBuilder.AppendLine("3. Focus EXCLUSIVELY on visual features, layout, and document structure.");
            promptBuilder.AppendLine("4. Document your complete visual analysis process thoroughly.");
            
            // Add visual analysis instructions with more emphasis on using only visual cues
            promptBuilder.AppendLine("\n### VISUAL-ONLY ELEMENT ANALYSIS ###");
            promptBuilder.AppendLine("1. DOCUMENT STRUCTURE: Analyze overall document layout and visual professionalism (construction invoices typically have structured layout with clear sections)");
            promptBuilder.AppendLine("2. LAYOUT PATTERN: Identify key section patterns visually (header area, line items table area, totals section area)");
            promptBuilder.AppendLine("3. VISUAL INDICATORS: Look for visual elements common in construction invoices (diagrams, technical symbols, architectural elements, material depictions)");
            promptBuilder.AppendLine("4. VISUAL CONSISTENCY: Assess visual consistency in formatting, alignment, and spacing");
            promptBuilder.AppendLine("5. CONSTRUCTION VISUAL SIGNATURES: Look for visual patterns uniquely common in construction/home improvement invoices versus retail receipts");
            
            // Include output format with strong emphasis on visual-only analysis
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidenceOverall\": 0-100,");
            promptBuilder.AppendLine("  \"visualAssessments\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"elementName\": \"DOCUMENT STRUCTURE\",");
            promptBuilder.AppendLine("      \"score\": 0-100,");
            promptBuilder.AppendLine("      \"confidence\": 0-100,");
            promptBuilder.AppendLine("      \"evidence\": \"Description of VISUAL evidence only\",");
            promptBuilder.AppendLine("      \"reasoning\": \"Visual analysis explanation\"");
            promptBuilder.AppendLine("    },");
            promptBuilder.AppendLine("    ... repeat for each visual element ...");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"possibleFraud\": true/false,");
            promptBuilder.AppendLine("  \"auditJustification\": \"Comprehensive explanation of VISUAL-ONLY analysis for audit purposes\"");
            promptBuilder.AppendLine("}");
            
            promptBuilder.AppendLine("\n### IMPORTANT REMINDER ###");
            promptBuilder.AppendLine("You must ONLY analyze the VISUAL aspects of the invoice. DO NOT try to read the text content or extract specific data.");
            promptBuilder.AppendLine("Focus EXCLUSIVELY on how the document LOOKS, its structure, layout, and visual patterns.");
            
            _advancedLogger.LogDebug("Generated visual-only analysis prompt with {CharacterCount} characters", promptBuilder.Length);
            
            return promptBuilder.ToString();
        }
        
        private string BuildAuditReadyPrompt(Invoice invoice)
        {
            _advancedLogger.LogDebug("Building audit-ready assessment prompt for {FileName}", invoice.FileName);
            
            var promptBuilder = new StringBuilder();
            
            // === HEADER WITH AUDIT METADATA ===
            promptBuilder.AppendLine("### VISUAL AUDIT REFERENCE ###");
            promptBuilder.AppendLine($"Audit Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            promptBuilder.AppendLine("Validator Version: Gemini Flash 2.0 with Visual-Only Audit v1.0");
            promptBuilder.AppendLine("Validation Framework: Dutch Construction Invoice Visual Assessment 2025");
            promptBuilder.AppendLine("Regulatory Context: Dutch Mortgage Credit Directive, BouwDepot Regulations");
            
            promptBuilder.AppendLine("\n### TASK: VISUAL INVOICE AUDIT ###");
            promptBuilder.AppendLine("You are a specialized visual auditor for Dutch construction and home improvement expenses.");
            promptBuilder.AppendLine("Your task is to analyze ONLY THE VISUAL ELEMENTS of this invoice to determine if it's related to home improvement.");
            promptBuilder.AppendLine("DO NOT read or analyze the text content directly - focus exclusively on visual patterns, layout, and document structure.");
            promptBuilder.AppendLine("IMPORTANT: Provide comprehensive documentation of your visual analysis process for audit compliance.");
            
            promptBuilder.AppendLine("\n### REGULATORY FRAMEWORK ###");
            promptBuilder.AppendLine("Dutch regulations require thorough documentation of invoice validation processes, including:");
            promptBuilder.AppendLine("- Visual consistency assessment");
            promptBuilder.AppendLine("- Document structure analysis");
            promptBuilder.AppendLine("- Visual tampering detection");
            promptBuilder.AppendLine("- Construction-specific visual pattern recognition");
            promptBuilder.AppendLine("- Detailed justification for approval or rejection decisions");
            promptBuilder.AppendLine("- Explicit explanation of specific rules that were met or violated");
            
            promptBuilder.AppendLine("\n### VISUAL ASSESSMENT CRITERIA ###");
            promptBuilder.AppendLine("Below are the specific visual criteria to assess:");
            promptBuilder.AppendLine("1. DOCUMENT STRUCTURE: Professional layout consistent with construction invoices (Weight: 20%)");
            promptBuilder.AppendLine("2. VISUAL ELEMENTS: Presence of construction-related visual elements (diagrams, technical symbols) (Weight: 30%)");
            promptBuilder.AppendLine("3. VISUAL CONSISTENCY: Consistent formatting, fonts, and alignment (Weight: 15%)");
            promptBuilder.AppendLine("4. TAMPER EVIDENCE: No visual indications of editing or manipulation (Weight: 25%)");
            promptBuilder.AppendLine("5. CONSTRUCTION PATTERNS: Visual patterns matching known construction invoice templates (Weight: 10%)");
            
            promptBuilder.AppendLine("\n### DETAILED ANALYSIS INSTRUCTIONS ###");
            promptBuilder.AppendLine("Conduct a thorough visual assessment following the audit-compliant methodology below:");
            promptBuilder.AppendLine("1. INDEPENDENT CRITERIA EVALUATION - Evaluate each visual criterion separately with evidence");
            promptBuilder.AppendLine("2. EVIDENCE DOCUMENTATION - Document specific visual evidence from the invoice");
            promptBuilder.AppendLine("3. CONFIDENCE ASSESSMENT - Assign confidence scores (0-100%) for each visual criterion");
            promptBuilder.AppendLine("4. WEIGHTED CALCULATION - Calculate weighted scores based on visual assessment");
            promptBuilder.AppendLine("5. VISUAL PATTERN RECOGNITION - Identify visual patterns common in construction invoices");
            promptBuilder.AppendLine("6. FINAL DETERMINATION - Make final determination with threshold of 70% for approval");
            promptBuilder.AppendLine("7. DETAILED JUSTIFICATION - Provide comprehensive explanation for approval or rejection");

            promptBuilder.AppendLine("\n### APPROVAL/REJECTION JUSTIFICATION REQUIREMENTS ###");
            promptBuilder.AppendLine("For any invoice that is accepted or rejected, provide a detailed justification that includes:");
            promptBuilder.AppendLine("1. PRIMARY FACTORS - List the most significant factors that led to approval or rejection");
            promptBuilder.AppendLine("2. SPECIFIC RULES - Identify the specific Bouwdepot rules that were met or violated:");
            promptBuilder.AppendLine("   a. QUALITY IMPROVEMENT RULE - Whether items improve the quality/value of the home");
            promptBuilder.AppendLine("   b. PERMANENT ATTACHMENT RULE - Whether items are permanently attached to property");
            promptBuilder.AppendLine("   c. VERDUURZAMINGSDEPOT CRITERIA - Whether items qualify for sustainability funding");
            promptBuilder.AppendLine("3. LINE ITEM ANALYSIS - For rejections, specify which line items failed to meet criteria");
            promptBuilder.AppendLine("4. FRAUD INDICATORS - Detail any suspicious elements detected (if applicable)");
            promptBuilder.AppendLine("5. CONFIDENCE ASSESSMENT - Explicitly state confidence level in the decision");
            promptBuilder.AppendLine("6. RECOMMENDED ACTIONS - For rejections, suggest possible remediation steps");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"auditMetadata\": {");
            promptBuilder.AppendLine("    \"validationTimestamp\": \"YYYY-MM-DD HH:MM:SS\",");
            promptBuilder.AppendLine("    \"frameworkVersion\": \"Dutch Construction Visual Assessment 2025\",");
            promptBuilder.AppendLine("    \"modelVersion\": \"Gemini Flash 2.0 Visual Audit\"");
            promptBuilder.AppendLine("  },");
            promptBuilder.AppendLine("  \"visualCriteriaAssessments\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"criterionName\": \"DOCUMENT STRUCTURE\",");
            promptBuilder.AppendLine("      \"weight\": 0.20,");
            promptBuilder.AppendLine("      \"evidence\": \"Description of visual evidence only\",");
            promptBuilder.AppendLine("      \"score\": 0-100,");
            promptBuilder.AppendLine("      \"confidence\": 0-100,");
            promptBuilder.AppendLine("      \"reasoning\": \"Visual analysis explanation\"");
            promptBuilder.AppendLine("    },");
            promptBuilder.AppendLine("    ... repeat for each visual criterion ...");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"weightedScore\": 0-100,");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidenceOverall\": 0-100,");
            promptBuilder.AppendLine("  \"likelyCategory\": \"Based on visual patterns: Roofing/Plumbing/etc.\",");
            promptBuilder.AppendLine("  \"auditJustification\": \"Comprehensive explanation of visual analysis for audit purposes\",");
            promptBuilder.AppendLine("  \"approvalFactors\": [\"List of key factors supporting approval\"],");
            promptBuilder.AppendLine("  \"rejectionFactors\": [\"List of key factors leading to rejection\"],");
            promptBuilder.AppendLine("  \"metRules\": [\"List of Bouwdepot rules that were met\"],");
            promptBuilder.AppendLine("  \"violatedRules\": [\"List of Bouwdepot rules that were violated\"],");
            promptBuilder.AppendLine("  \"recommendedActions\": [\"Suggestions to address rejections\"],");
            promptBuilder.AppendLine("  \"visualRegulationNotes\": \"Visual compliance considerations for audit purposes\"");
            promptBuilder.AppendLine("}");
            
            _advancedLogger.LogDebug("Generated audit-ready assessment prompt with {CharacterCount} characters", promptBuilder.Length);
            
            return promptBuilder.ToString();
        }
        
        private void ParseMultiModalResponse(string responseText, ValidationResult result)
        {
            _advancedLogger.LogDebug("Parsing multi-modal analysis response");
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<MultiModalResponse>(responseText);
                
                if (response != null)
                {
                    _advancedLogger.LogInformation("Multi-modal analysis: IsHomeImprovement={IsHomeImprovement}, Confidence={Confidence}, PossibleFraud={PossibleFraud}", 
                        response.IsHomeImprovement, response.ConfidenceOverall, response.PossibleFraud);
                    
                    // Set basic validation results
                    result.IsHomeImprovement = response.IsHomeImprovement;
                    result.IsValid = response.IsHomeImprovement && !response.PossibleFraud;
                    result.AuditJustification = response.AuditJustification;
                    
                    // Add visual assessments
                    if (response.VisualAssessments != null)
                    {
                        _advancedLogger.LogInformation("Processing {Count} visual assessments", response.VisualAssessments.Count);
                        
                        foreach (var assessment in response.VisualAssessments)
                        {
                            result.VisualAssessments.Add(new VisualAssessment
                            {
                                ElementName = assessment.ElementName,
                                Score = assessment.Score,
                                Confidence = assessment.Confidence,
                                Evidence = assessment.Evidence,
                                Reasoning = assessment.Reasoning
                            });
                            
                            _advancedLogger.LogDebug("Visual assessment - {ElementName}: Score={Score}, Confidence={Confidence}", 
                                assessment.ElementName, assessment.Score, assessment.Confidence);
                            
                            // Add as informational issue for user interface
                            result.AddIssue(ValidationSeverity.Info, 
                                $"Visual assessment - {assessment.ElementName}: {assessment.Reasoning}");
                        }
                    }
                    
                    // Add textual assessments
                    if (response.TextualAssessments != null && response.TextualAssessments.Count > 0)
                    {
                        _advancedLogger.LogInformation("Processing {Count} textual assessments", response.TextualAssessments.Count);
                        
                        foreach (var assessment in response.TextualAssessments)
                        {
                            result.CriteriaAssessments.Add(new CriterionAssessment
                            {
                                CriterionName = assessment.CriterionName,
                                Score = assessment.Score,
                                Confidence = assessment.Confidence,
                                Evidence = assessment.Evidence,
                                Reasoning = assessment.Reasoning
                            });
                            
                            _advancedLogger.LogDebug("Text assessment - {CriterionName}: Score={Score}, Confidence={Confidence}", 
                                assessment.CriterionName, assessment.Score, assessment.Confidence);
                            
                            // Add as informational issue for user interface
                            result.AddIssue(ValidationSeverity.Info, 
                                $"Text assessment - {assessment.CriterionName}: {assessment.Reasoning}");
                        }
                    }
                    
                    // If fraud is detected, add it as an error
                    if (response.PossibleFraud)
                    {
                        _advancedLogger.LogWarning("Multi-modal analysis detected potential fraud");
                        
                        result.AddIssue(ValidationSeverity.Error, 
                            "Multi-modal analysis detected potential fraud in this invoice.");
                    }
                }
                else
                {
                    _advancedLogger.LogWarning("Could not parse multi-modal response JSON");
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Could not properly parse the multi-modal analysis results.");
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                _advancedLogger.LogError(ex, "Error parsing multi-modal response: {ErrorMessage}", ex.Message);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error processing multi-modal analysis results: {ex.Message}");
                result.IsValid = false;
            }
        }
        
        private void ParseAuditReadyResponse(string responseText, ValidationResult result)
        {
            _advancedLogger.LogDebug("Parsing audit-ready assessment response");
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<AuditReadyResponse>(responseText);
                
                if (response != null)
                {
                    _advancedLogger.LogInformation("Audit assessment: IsHomeImprovement={IsHomeImprovement}, WeightedScore={WeightedScore}, Confidence={Confidence}", 
                        response.IsHomeImprovement, response.WeightedScore, response.ConfidenceOverall);
                    
                    if (!string.IsNullOrEmpty(response.SpecificCategory))
                    {
                        _advancedLogger.LogInformation("Detected category: {Category}", response.SpecificCategory);
                    }
                    
                    // Set basic validation results
                    result.IsHomeImprovement = response.IsHomeImprovement;
                    result.IsValid = response.IsHomeImprovement && response.WeightedScore >= 70;
                    result.WeightedScore = (int)response.WeightedScore;
                    result.AuditJustification = response.AuditJustification;
                    result.RegulatoryNotes = new List<string> { response.RegulatoryNotes };
                    
                    // Add detailed approval/rejection factors
                    if (response.ApprovalFactors != null && response.ApprovalFactors.Count > 0)
                    {
                        _advancedLogger.LogInformation("Adding {Count} approval factors", response.ApprovalFactors.Count);
                        result.ApprovalFactors = response.ApprovalFactors;
                    }
                    
                    if (response.RejectionFactors != null && response.RejectionFactors.Count > 0)
                    {
                        _advancedLogger.LogInformation("Adding {Count} rejection factors", response.RejectionFactors.Count);
                        result.RejectionFactors = response.RejectionFactors;
                    }
                    
                    // Add rule compliance information
                    if (response.MetRules != null && response.MetRules.Count > 0)
                    {
                        _advancedLogger.LogInformation("Adding {Count} met rules", response.MetRules.Count);
                        result.MetRules = response.MetRules;
                    }
                    
                    if (response.ViolatedRules != null && response.ViolatedRules.Count > 0)
                    {
                        _advancedLogger.LogInformation("Adding {Count} violated rules", response.ViolatedRules.Count);
                        result.ViolatedRules = response.ViolatedRules;
                        
                        // Add violated rules as warning issues
                        foreach (var rule in response.ViolatedRules)
                        {
                            result.AddIssue(ValidationSeverity.Warning, $"Rule violation: {rule}");
                        }
                    }
                    
                    // Add recommended actions if available
                    if (response.RecommendedActions != null && response.RecommendedActions.Count > 0)
                    {
                        _advancedLogger.LogInformation("Adding {Count} recommended actions", response.RecommendedActions.Count);
                        result.RecommendedActions = response.RecommendedActions;
                        
                        // Add recommended actions as info issues
                        foreach (var action in response.RecommendedActions)
                        {
                            result.AddIssue(ValidationSeverity.Info, $"Recommended action: {action}");
                        }
                    }
                    
                    // Set audit metadata
                    if (response.AuditMetadata != null)
                    {
                        try
                        {
                            _advancedLogger.LogDebug("Processing audit metadata with timestamp: {Timestamp}", 
                                response.AuditMetadata.ValidationTimestamp);
                                
                            result.AuditInfo = new AuditMetadata
                            {
                                TimestampUtc = DateTime.Parse(response.AuditMetadata.ValidationTimestamp),
                                FrameworkVersion = response.AuditMetadata.FrameworkVersion,
                                ModelVersion = response.AuditMetadata.ModelVersion
                            };
                        }
                        catch (Exception ex)
                        {
                            _advancedLogger.LogWarning(ex, "Error parsing audit metadata timestamp: {ErrorMessage}", ex.Message);
                            
                            result.AuditInfo = new AuditMetadata
                            {
                                TimestampUtc = DateTime.UtcNow,
                                FrameworkVersion = "Dutch Residential Construction Assessment 2025",
                                ModelVersion = "Gemini Flash 2.0"
                            };
                        }
                    }
                    
                    // Add criteria assessments
                    if (response.CriteriaAssessments != null)
                    {
                        _advancedLogger.LogInformation("Processing {Count} audit criteria assessments", response.CriteriaAssessments.Count);
                        
                        foreach (var assessment in response.CriteriaAssessments)
                        {
                            result.CriteriaAssessments.Add(new CriterionAssessment
                            {
                                CriterionName = assessment.CriterionName,
                                Weight = assessment.Weight,
                                Evidence = assessment.Evidence,
                                Score = assessment.Score,
                                Confidence = assessment.Confidence,
                                Reasoning = assessment.Reasoning
                            });
                            
                            _advancedLogger.LogDebug("Criteria assessment - {CriterionName}: Score={Score}, Weight={Weight}", 
                                assessment.CriterionName, assessment.Score, assessment.Weight);
                            
                            // Add as informational issue for user interface with severity based on score
                            var severity = assessment.Score < 50 ? ValidationSeverity.Warning : ValidationSeverity.Info;
                            result.AddIssue(severity, 
                                $"{assessment.CriterionName}: {assessment.Reasoning} (Score: {assessment.Score}/100)");
                        }
                    }
                    
                    // Add overall result
                    if (result.IsHomeImprovement)
                    {
                        _advancedLogger.LogInformation("Invoice classified as valid home improvement expense in category {Category} with score {Score}", 
                            response.SpecificCategory, response.WeightedScore);
                            
                        result.AddIssue(ValidationSeverity.Info, 
                            $"This invoice is classified as a valid home improvement expense ({response.WeightedScore:F0}/100) in category: {response.SpecificCategory}");
                    }
                    else
                    {
                        _advancedLogger.LogWarning("Invoice not classified as home improvement expense, score: {Score}", response.WeightedScore);
                        
                        result.AddIssue(ValidationSeverity.Error, 
                            $"This invoice does not qualify as a home improvement expense ({response.WeightedScore:F0}/100)");
                    }
                }
                else
                {
                    _advancedLogger.LogWarning("Could not parse audit-ready response JSON");
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Could not properly parse the audit-ready analysis results.");
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                _advancedLogger.LogError(ex, "Error parsing audit-ready response: {ErrorMessage}", ex.Message);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error processing audit-ready results: {ex.Message}");
                result.IsValid = false;
            }
        }
        
        internal class MultiModalResponse
        {
            [JsonPropertyName("isHomeImprovement")]
            public bool IsHomeImprovement { get; set; }
            
            [JsonPropertyName("confidenceOverall")]
            public int ConfidenceOverall { get; set; }
            
            [JsonPropertyName("visualAssessments")]
            public List<VisualAssessmentResponse> VisualAssessments { get; set; } = new List<VisualAssessmentResponse>();
            
            [JsonPropertyName("textualAssessments")]
            public List<TextualAssessmentResponse> TextualAssessments { get; set; } = new List<TextualAssessmentResponse>();
            
            [JsonPropertyName("possibleFraud")]
            public bool PossibleFraud { get; set; }
            
            [JsonPropertyName("auditJustification")]
            public string AuditJustification { get; set; } = string.Empty;
        }
        
        internal class VisualAssessmentResponse
        {
            [JsonPropertyName("elementName")]
            public string ElementName { get; set; } = string.Empty;
            
            [JsonPropertyName("score")]
            public int Score { get; set; }
            
            [JsonPropertyName("confidence")]
            public int Confidence { get; set; }
            
            [JsonPropertyName("evidence")]
            public string Evidence { get; set; } = string.Empty;
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
        }
        
        internal class TextualAssessmentResponse
        {
            [JsonPropertyName("criterionName")]
            public string CriterionName { get; set; } = string.Empty;
            
            [JsonPropertyName("score")]
            public int Score { get; set; }
            
            [JsonPropertyName("confidence")]
            public int Confidence { get; set; }
            
            [JsonPropertyName("evidence")]
            public string Evidence { get; set; } = string.Empty;
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
        }
        
        internal class AuditReadyResponse
        {
            [JsonPropertyName("auditMetadata")]
            public AuditMetadataResponse AuditMetadata { get; set; } = new AuditMetadataResponse();
            
            [JsonPropertyName("visualCriteriaAssessments")]
            public List<CriteriaAssessmentResponse> CriteriaAssessments { get; set; } = new List<CriteriaAssessmentResponse>();
            
            [JsonPropertyName("weightedScore")]
            public decimal WeightedScore { get; set; }
            
            [JsonPropertyName("isHomeImprovement")]
            public bool IsHomeImprovement { get; set; }
            
            [JsonPropertyName("confidenceOverall")]
            public int ConfidenceOverall { get; set; }
            
            [JsonPropertyName("likelyCategory")]
            public string SpecificCategory { get; set; } = string.Empty;
            
            [JsonPropertyName("auditJustification")]
            public string AuditJustification { get; set; } = string.Empty;
            
            [JsonPropertyName("approvalFactors")]
            public List<string> ApprovalFactors { get; set; } = new List<string>();
            
            [JsonPropertyName("rejectionFactors")]
            public List<string> RejectionFactors { get; set; } = new List<string>();
            
            [JsonPropertyName("metRules")]
            public List<string> MetRules { get; set; } = new List<string>();
            
            [JsonPropertyName("violatedRules")]
            public List<string> ViolatedRules { get; set; } = new List<string>();
            
            [JsonPropertyName("recommendedActions")]
            public List<string> RecommendedActions { get; set; } = new List<string>();
            
            [JsonPropertyName("visualRegulationNotes")]
            public string RegulatoryNotes { get; set; } = string.Empty;
        }
        
        internal class AuditMetadataResponse
        {
            [JsonPropertyName("validationTimestamp")]
            public string ValidationTimestamp { get; set; } = string.Empty;
            
            [JsonPropertyName("frameworkVersion")]
            public string FrameworkVersion { get; set; } = string.Empty;
            
            [JsonPropertyName("modelVersion")]
            public string ModelVersion { get; set; } = string.Empty;
        }
        
        internal class CriteriaAssessmentResponse
        {
            [JsonPropertyName("criterionName")]
            public string CriterionName { get; set; } = string.Empty;
            
            [JsonPropertyName("weight")]
            public decimal Weight { get; set; }
            
            [JsonPropertyName("evidence")]
            public string Evidence { get; set; } = string.Empty;
            
            [JsonPropertyName("score")]
            public int Score { get; set; }
            
            [JsonPropertyName("confidence")]
            public int Confidence { get; set; }
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
        }
        
        private string BuildInvoiceDataExtractionPrompt()
        {
            _advancedLogger.LogDebug("Building invoice data extraction prompt");
            
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### INVOICE DATA EXTRACTION ###");
            promptBuilder.AppendLine("You are an invoice data extraction specialist trained to extract key invoice information from images.");
            promptBuilder.AppendLine("I'm attaching images of an invoice document. Extract the following information from the images:");
            promptBuilder.AppendLine("1. Invoice number");
            promptBuilder.AppendLine("2. Invoice date");
            promptBuilder.AppendLine("3. Total amount");
            promptBuilder.AppendLine("4. VAT amount (if present)");
            promptBuilder.AppendLine("5. Vendor name (company issuing the invoice)");
            promptBuilder.AppendLine("6. Vendor address");
            promptBuilder.AppendLine("7. Vendor KvK number (Dutch Chamber of Commerce number, if present)");
            promptBuilder.AppendLine("8. Vendor BTW number (Dutch VAT number, if present)");
            promptBuilder.AppendLine("9. Line items (description, quantity, unit price, total price)");
            
            promptBuilder.AppendLine("\n### IMPORTANT EXTRACTION GUIDELINES ###");
            promptBuilder.AppendLine("- For the invoice number, look for text labeled 'Invoice #', 'Factuurnummer', 'Factuur nr', etc.");
            promptBuilder.AppendLine("- For the invoice date, identify dates in formats like DD-MM-YYYY, MM/DD/YYYY, or written formats");
            promptBuilder.AppendLine("- For the total amount, look for terms like 'Total', 'Totaal', 'Amount Due', 'Te betalen', etc.");
            promptBuilder.AppendLine("- Remove any currency symbols from numeric values");
            promptBuilder.AppendLine("- Use the decimal format for currency (e.g., 1234.56)");
            promptBuilder.AppendLine("- For dates, use the format YYYY-MM-DD regardless of the format in the original document");
            promptBuilder.AppendLine("- If you can't find a specific field, return an empty string or null");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"invoiceNumber\": \"string\",");
            promptBuilder.AppendLine("  \"invoiceDate\": \"YYYY-MM-DD\",");
            promptBuilder.AppendLine("  \"totalAmount\": number,");
            promptBuilder.AppendLine("  \"vatAmount\": number,");
            promptBuilder.AppendLine("  \"vendorName\": \"string\",");
            promptBuilder.AppendLine("  \"vendorAddress\": \"string\",");
            promptBuilder.AppendLine("  \"vendorKvkNumber\": \"string\",");
            promptBuilder.AppendLine("  \"vendorBtwNumber\": \"string\",");
            promptBuilder.AppendLine("  \"lineItems\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"string\",");
            promptBuilder.AppendLine("      \"quantity\": number,");
            promptBuilder.AppendLine("      \"unitPrice\": number,");
            promptBuilder.AppendLine("      \"totalPrice\": number,");
            promptBuilder.AppendLine("      \"vatRate\": number");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ]");
            promptBuilder.AppendLine("}");
            
            _advancedLogger.LogDebug("Generated invoice data extraction prompt with {CharacterCount} characters", promptBuilder.Length);
            
            return promptBuilder.ToString();
        }
        
        private void ParseInvoiceDataExtractionResponse(string responseText, Invoice invoice)
        {
            _advancedLogger.LogDebug("Parsing invoice data extraction response");
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<InvoiceDataExtractionResponse>(responseText);
                
                if (response != null)
                {
                    // Log successful extraction
                    _advancedLogger.LogInformation("Successfully extracted invoice data from response: " +
                                                  "Number={InvoiceNumber}, Date={InvoiceDate}, Amount={TotalAmount}", 
                                                  response.InvoiceNumber, response.InvoiceDate, response.TotalAmount);
                    
                    // Set invoice properties
                    invoice.InvoiceNumber = response.InvoiceNumber ?? string.Empty;
                    
                    // Parse date with proper handling
                    if (!string.IsNullOrEmpty(response.InvoiceDate))
                    {
                        if (DateTime.TryParse(response.InvoiceDate, out DateTime date))
                        {
                            invoice.InvoiceDate = date;
                            _advancedLogger.LogDebug("Parsed invoice date: {Date}", date);
                        }
                        else
                        {
                            _advancedLogger.LogWarning("Failed to parse invoice date: {DateString}", response.InvoiceDate);
                        }
                    }
                    
                    // Set amounts
                    if (response.TotalAmount > 0)
                    {
                        invoice.TotalAmount = response.TotalAmount;
                        _advancedLogger.LogDebug("Extracted total amount: {Amount}", response.TotalAmount);
                    }
                    
                    if (response.VatAmount > 0)
                    {
                        invoice.VatAmount = response.VatAmount;
                        _advancedLogger.LogDebug("Extracted VAT amount: {Amount}", response.VatAmount);
                    }
                    
                    // Set vendor details
                    invoice.VendorName = response.VendorName ?? string.Empty;
                    invoice.VendorAddress = response.VendorAddress ?? string.Empty;
                    invoice.VendorKvkNumber = response.VendorKvkNumber ?? string.Empty;
                    invoice.VendorBtwNumber = response.VendorBtwNumber ?? string.Empty;
                    
                    if (!string.IsNullOrEmpty(invoice.VendorName))
                    {
                        _advancedLogger.LogDebug("Extracted vendor name: {Name}", invoice.VendorName);
                    }
                    
                    // Add line items
                    if (response.LineItems != null && response.LineItems.Count > 0)
                    {
                        // Clear existing items to avoid duplicates
                        invoice.LineItems.Clear();
                        
                        _advancedLogger.LogInformation("Processing {Count} extracted line items", response.LineItems.Count);
                        
                        foreach (var item in response.LineItems)
                        {
                            var lineItem = new InvoiceLineItem
                            {
                                Description = item.Description ?? string.Empty,
                                Quantity = item.Quantity > 0 ? item.Quantity : 1,
                                UnitPrice = item.UnitPrice,
                                TotalPrice = item.TotalPrice > 0 ? item.TotalPrice : (item.UnitPrice * item.Quantity),
                                VatRate = item.VatRate > 0 ? item.VatRate : 21 // Default Dutch high rate
                            };
                            
                            invoice.LineItems.Add(lineItem);
                            
                            _advancedLogger.LogDebug("Added line item: Description={Description}, Quantity={Quantity}, TotalPrice={TotalPrice}", 
                                lineItem.Description, lineItem.Quantity, lineItem.TotalPrice);
                        }
                        
                        _advancedLogger.LogInformation("Added {Count} line items to invoice", invoice.LineItems.Count);
                    }
                }
                else
                {
                    _advancedLogger.LogWarning("Failed to parse invoice data extraction response");
                }
            }
            catch (Exception ex)
            {
                _advancedLogger.LogError(ex, "Error parsing invoice data extraction response: {ErrorMessage}", ex.Message);
            }
        }
        
        internal class InvoiceDataExtractionResponse
        {
            [JsonPropertyName("invoiceNumber")]
            public string? InvoiceNumber { get; set; }
            
            [JsonPropertyName("invoiceDate")]
            public string? InvoiceDate { get; set; }
            
            [JsonPropertyName("totalAmount")]
            public decimal TotalAmount { get; set; }
            
            [JsonPropertyName("vatAmount")]
            public decimal VatAmount { get; set; }
            
            [JsonPropertyName("vendorName")]
            public string? VendorName { get; set; }
            
            [JsonPropertyName("vendorAddress")]
            public string? VendorAddress { get; set; }
            
            [JsonPropertyName("vendorKvkNumber")]
            public string? VendorKvkNumber { get; set; }
            
            [JsonPropertyName("vendorBtwNumber")]
            public string? VendorBtwNumber { get; set; }
            
            [JsonPropertyName("lineItems")]
            public List<LineItemResponse> LineItems { get; set; } = new List<LineItemResponse>();
        }
        
        internal class LineItemResponse
        {
            [JsonPropertyName("description")]
            public string? Description { get; set; }
            
            // Use JsonElement instead of int to handle flexible types
            [JsonPropertyName("quantity")]
            public JsonElement QuantityElement { get; set; }
            
            // Calculated property that attempts to convert to int safely
            public int Quantity 
            { 
                get
                {
                    try
                    {
                        if (QuantityElement.ValueKind == JsonValueKind.Number)
                        {
                            return QuantityElement.GetInt32();
                        }
                        else if (QuantityElement.ValueKind == JsonValueKind.String)
                        {
                            string value = QuantityElement.GetString() ?? "1";
                            if (int.TryParse(value, out int result))
                            {
                                return result;
                            }
                            
                            // Try to parse it as a decimal then convert to int
                            if (decimal.TryParse(value, out decimal decimalResult))
                            {
                                return (int)decimalResult;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Log but don't throw, return default
                    }
                    return 1; // Default to 1 if parsing fails
                }
            }
            
            // Use JsonElement for other numeric fields too for flexibility
            [JsonPropertyName("unitPrice")]
            public JsonElement UnitPriceElement { get; set; }
            
            public decimal UnitPrice
            {
                get
                {
                    try
                    {
                        if (UnitPriceElement.ValueKind == JsonValueKind.Number)
                        {
                            return UnitPriceElement.GetDecimal();
                        }
                        else if (UnitPriceElement.ValueKind == JsonValueKind.String)
                        {
                            string value = UnitPriceElement.GetString() ?? "0";
                            if (decimal.TryParse(value, out decimal result))
                            {
                                return result;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Log but don't throw, return default
                    }
                    return 0; // Default to 0 if parsing fails
                }
            }
            
            [JsonPropertyName("totalPrice")]
            public JsonElement TotalPriceElement { get; set; }
            
            public decimal TotalPrice
            {
                get
                {
                    try
                    {
                        if (TotalPriceElement.ValueKind == JsonValueKind.Number)
                        {
                            return TotalPriceElement.GetDecimal();
                        }
                        else if (TotalPriceElement.ValueKind == JsonValueKind.String)
                        {
                            string value = TotalPriceElement.GetString() ?? "0";
                            if (decimal.TryParse(value, out decimal result))
                            {
                                return result;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Log but don't throw, return default
                    }
                    return 0; // Default to 0 if parsing fails
                }
            }
            
            [JsonPropertyName("vatRate")]
            public JsonElement VatRateElement { get; set; }
            
            public decimal VatRate
            {
                get
                {
                    try
                    {
                        if (VatRateElement.ValueKind == JsonValueKind.Number)
                        {
                            return VatRateElement.GetDecimal();
                        }
                        else if (VatRateElement.ValueKind == JsonValueKind.String)
                        {
                            string value = VatRateElement.GetString() ?? "21";
                            if (decimal.TryParse(value, out decimal result))
                            {
                                return result;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Log but don't throw, return default
                    }
                    return 21; // Default to standard Dutch VAT rate if parsing fails
                }
            }
        }
    }
}
