using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Models.Analysis; // Added using directive

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for performing comprehensive, multi-faceted analysis of invoices using all Gemini AI services
    /// </summary>
    public class GeminiAdvancedAnalysisService : GeminiServiceBase
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<GeminiAdvancedAnalysisService> _logger;
        private readonly GeminiDocumentAnalysisService _documentService;
        private readonly GeminiHomeImprovementService _homeImprovementService;
        private readonly GeminiFraudDetectionService _fraudDetectionService;
        private readonly GeminiLineItemAnalysisService _lineItemService;

        public GeminiAdvancedAnalysisService(
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
            : base(loggerFactory.CreateLogger<GeminiServiceBase>(), configuration) // Pass base logger
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<GeminiAdvancedAnalysisService>();
            
            // Initialize specialized services
            _documentService = new GeminiDocumentAnalysisService(
                _loggerFactory.CreateLogger<GeminiDocumentAnalysisService>(), configuration);
                
            _homeImprovementService = new GeminiHomeImprovementService(
                _loggerFactory.CreateLogger<GeminiHomeImprovementService>(), configuration);
                
            _fraudDetectionService = new GeminiFraudDetectionService(
                _loggerFactory.CreateLogger<GeminiFraudDetectionService>(), configuration);
                
            _lineItemService = new GeminiLineItemAnalysisService(
                _loggerFactory.CreateLogger<GeminiLineItemAnalysisService>(), configuration);
        }

        /// <summary>
        /// Performs a comprehensive analysis of an invoice using all available Gemini AI capabilities
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <returns>A comprehensive response containing all analysis results</returns>
        public async Task<GeminiComprehensiveResponse> PerformComprehensiveAnalysisAsync(Invoice invoice)
        {
            _logger.LogInformation("Starting comprehensive invoice analysis");
            
            var response = new GeminiComprehensiveResponse
            {
                AnalysisStartTime = DateTime.Now
            };
            
            try
            {
                // Step 1: Document verification and extraction (if needed)
                if (string.IsNullOrEmpty(invoice.InvoiceNumber) || !invoice.InvoiceDate.HasValue || invoice.TotalAmount <= 0)
                {
                    _logger.LogInformation("Invoice data incomplete, performing extraction from images");
                    
                    if (invoice.PageImages != null && invoice.PageImages.Count > 0)
                    {
                        try
                        {
                            invoice = await _documentService.ExtractInvoiceDataFromImagesAsync(invoice);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error extracting invoice data from images");
                            response.AddValidationIssue(ValidationSeverity.Error, 
                                $"Failed to extract invoice data from images: {ex.Message}");
                        }
                    }
                }
                
                // Step 2: Run multiple analyses in parallel for efficiency
                var documentTask = VerifyDocumentAsync(invoice, response);
                var homeImprovementTask = AnalyzeHomeImprovementAsync(invoice, response);
                var lineItemTask = AnalyzeLineItemsAsync(invoice, response);
                
                // Wait for all tasks to complete, regardless of success/failure
                // Removed LogTaskError calls as the tasks do not return Task<T>
                await Task.WhenAll(
                    documentTask,
                    homeImprovementTask,
                    lineItemTask
                );

                // Step 3: Run fraud detection (depends on document verification)
                try
                {
                    bool detectedTampering = response.PossibleFraud;
                    await AnalyzeFraudAsync(invoice, detectedTampering, response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during fraud detection analysis");
                    response.AddValidationIssue(ValidationSeverity.Error, 
                        $"Failed to perform fraud detection: {ex.Message}");
                }
                
                // Step 4: Get audit-ready assessment if requested
                try
                {
                    await GetAuditAssessmentAsync(invoice, response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during audit assessment");
                    response.AddValidationIssue(ValidationSeverity.Error, 
                        $"Failed to generate audit assessment: {ex.Message}");
                }
                
                // Step 5: Summarize and finalize results
                response.SummarizeResults();
                
                _logger.LogInformation("Comprehensive analysis completed: IsValid={IsValid}, ConfidenceScore={ConfidenceScore}", 
                    response.IsValid, response.OverallConfidenceScore);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during comprehensive analysis");
                response.AddValidationIssue(ValidationSeverity.Error, 
                    $"Comprehensive analysis failed: {ex.Message}");
                
                response.MarkAnalysisComplete();
                response.IsValid = false;
                return response;
            }
        }

        /// <summary>
        /// Performs a focused analysis on a specific aspect of the invoice
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <param name="analysisType">The type of analysis to perform</param>
        /// <returns>A validation result containing the analysis</returns>
        public async Task<ValidationResult> PerformTargetedAnalysisAsync(Invoice invoice, string analysisType)
        {
            _logger.LogInformation("Performing targeted analysis: {AnalysisType}", analysisType);
            
            switch (analysisType.ToLowerInvariant())
            {
                case "document":
                    return await _documentService.VerifyDocumentTypeAsync(invoice);
                    
                case "homeimprovement":
                    return await _homeImprovementService.ValidateHomeImprovementAsync(invoice);
                    
                case "multimodal":
                    return await _homeImprovementService.ValidateWithMultiModalAnalysisAsync(invoice);
                    
                case "audit":
                    return await _lineItemService.GetAuditReadyAssessmentAsync(invoice);
                    
                default:
                    var result = new ValidationResult { ExtractedInvoice = invoice };
                    result.AddIssue(ValidationSeverity.Error, 
                        $"Unknown analysis type: {analysisType}");
                    result.IsValid = false;
                    return result;
            }
        }
        
        /// <summary>
        /// Maps a ValidationResult to the comprehensive response
        /// </summary>
        public void MapValidationResultToComprehensiveResponse(ValidationResult result, GeminiComprehensiveResponse response)
        {
            if (result == null) return;
            
            // Copy validation issues
            foreach (var issue in result.Issues)
            {
                response.AddValidationIssue(issue.Severity, issue.Message);
            }
            
            // Copy validity status
            response.IsValid = result.IsValid && response.IsValid;
            
            // Copy fraud detection data if available
            if (result.FraudDetection != null)
            {
                response.PossibleFraud = result.FraudDetection.FraudIndicatorsDetected;
                response.FraudRiskLevel = result.FraudDetection.RiskLevel.ToString();
                
                if (result.FraudDetection.DetectedIndicators != null)
                {
                    foreach (var indicator in result.FraudDetection.DetectedIndicators)
                    {
                        response.FraudIndicators.Add(indicator.Name);
                    }
                }
                
                response.FraudAssessmentExplanation = result.FraudDetection.Notes;
            }

            // Copy home improvement data - Reverted change, IsHomeImprovement is not nullable
            response.IsHomeImprovement = result.IsHomeImprovement;

            // Copy document validation data - Corrected nullable assignment
            response.IsValidInvoice = result.IsVerifiedInvoice ?? false;
            response.DetectedDocumentType = result.DetectedDocumentType;
            response.DocumentValidationConfidence = result.DocumentVerificationConfidence;

            if (result.PresentInvoiceElements != null)
            {
                response.PresentInvoiceElements = result.PresentInvoiceElements;
            }
            
            if (result.MissingInvoiceElements != null)
            {
                response.MissingInvoiceElements = result.MissingInvoiceElements;
            }
            
            // Copy visual assessment data - REMOVED incorrect assignment due to type mismatch and Obsolete attribute
            // if (result.VisualAssessments != null && result.VisualAssessments.Count > 0)
            // {
            //     // This assignment is incorrect: List<Models.VisualAssessment> vs List<Models.Analysis.DetailedVisualAssessment>
            //     // response.VisualAssessments = result.VisualAssessments; 
            //     response.HasVisualAnomalies = result.VisualAssessments.Exists(a => a.Score < 0);
            // }

            // Copy audit report data - REMOVED incorrect assignment due to type mismatch
            // if (result.AuditReport != null)
            // {
            //     // This assignment is incorrect: Models.AuditReport vs Models.Analysis.AuditAssessment
            //     // response.AuditAssessment = result.AuditReport;
            // }

            // Copy detailed reasoning
            if (!string.IsNullOrEmpty(result.DetailedReasoning))
            {
                response.DetailedReasoning = result.DetailedReasoning;
            }
            
            // Copy confidence score
            if (result.ConfidenceScore > 0)
            {
                // Use minimum of existing score and new score as "weakest link" approach
                if (response.OverallConfidenceScore > 0)
                {
                    response.OverallConfidenceScore = Math.Min(response.OverallConfidenceScore, result.ConfidenceScore);
                }
                else
                {
                    response.OverallConfidenceScore = result.ConfidenceScore;
                }
            }
        }
        
        private void LogTaskError<T>(Task<T> task, string analysisName)
        {
            if (task.IsFaulted && task.Exception != null)
            {
                _logger.LogError(task.Exception, "Error during {AnalysisName}", analysisName);
            }
        }

        #region Analysis Tasks

        private async Task VerifyDocumentAsync(Invoice invoice, GeminiComprehensiveResponse response)
        {
            try
            {
                var result = await _documentService.VerifyDocumentTypeAsync(invoice);
                
                // Store raw response
                response.DocumentAnalysisResponse = result.RawGeminiResponse;
                
                // Map validation result to comprehensive response
                MapValidationResultToComprehensiveResponse(result, response);
                
                _logger.LogInformation("Document verification completed: IsValid={IsValid}, Type={Type}", 
                    result.IsVerifiedInvoice, result.DetectedDocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document");
                response.AddValidationIssue(ValidationSeverity.Error, 
                    $"Document verification failed: {ex.Message}");
            }
        }
        
        private async Task AnalyzeHomeImprovementAsync(Invoice invoice, GeminiComprehensiveResponse response)
        {
            try
            {
                ValidationResult result;
                
                // Use multi-modal analysis if images are available
                if (invoice.PageImages != null && invoice.PageImages.Count > 0)
                {
                    result = await _homeImprovementService.ValidateWithMultiModalAnalysisAsync(invoice);
                    response.MultiModalResponse = result.RawGeminiResponse;
                }
                else
                {
                    result = await _homeImprovementService.ValidateHomeImprovementAsync(invoice);
                    response.HomeImprovementResponse = result.RawGeminiResponse;
                }
                
                // Map validation result to comprehensive response
                MapValidationResultToComprehensiveResponse(result, response);
                
                // Set specific home improvement properties
                response.HomeImprovementConfidence = result.ConfidenceScore / 100.0;
                
                if (result.PurchaseAnalysis != null)
                {
                    response.HomeImprovementCategory = result.PurchaseAnalysis.PrimaryPurpose;
                    response.HomeImprovementExplanation = result.PurchaseAnalysis.Summary;
                }
                
                _logger.LogInformation("Home improvement analysis completed: IsHomeImprovement={IsHomeImprovement}, Confidence={Confidence}", 
                    result.IsHomeImprovement, result.ConfidenceScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing home improvement relevance");
                response.AddValidationIssue(ValidationSeverity.Error, 
                    $"Home improvement analysis failed: {ex.Message}");
            }
        }
        
        private async Task AnalyzeLineItemsAsync(Invoice invoice, GeminiComprehensiveResponse response)
        {
            try
            {
                // Skip line item analysis if no line items are available
                if (invoice.LineItems == null || invoice.LineItems.Count == 0)
                {
                    _logger.LogWarning("Skipping line item analysis - no line items available");
                    response.AddValidationIssue(ValidationSeverity.Info, 
                        "No line items available for analysis");
                    return;
                }
                
                var result = await _lineItemService.AnalyzeLineItemsAsync(invoice);
                
                // Store raw response
                response.LineItemAnalysisResponse = result.RawResponse;
                
                // Store line item analysis
                response.LineItemAnalysis = result;
                
                // Set home improvement category from line item analysis if not already set
                if (string.IsNullOrEmpty(response.HomeImprovementCategory) &&
                    !string.IsNullOrEmpty(result.PrimaryCategory))
                {
                    response.HomeImprovementCategory = result.PrimaryCategory;
                }
                
                // Set home improvement confidence from line item analysis if not already set
                if (response.HomeImprovementConfidence == 0 && result.HomeImprovementRelevance > 0)
                {
                    response.HomeImprovementConfidence = result.HomeImprovementRelevance / 100.0;
                }
                
                // Use line item assessment for detailed reasoning if not already set
                if (string.IsNullOrEmpty(response.DetailedReasoning) && 
                    !string.IsNullOrEmpty(result.OverallAssessment))
                {
                    response.DetailedReasoning = result.OverallAssessment;
                }
                
                _logger.LogInformation("Line item analysis completed: PrimaryCategory={PrimaryCategory}, HomeImprovementRelevance={Relevance}", 
                    result.PrimaryCategory, result.HomeImprovementRelevance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing line items");
                response.AddValidationIssue(ValidationSeverity.Error, 
                    $"Line item analysis failed: {ex.Message}");
            }
        }
        
        private async Task AnalyzeFraudAsync(Invoice invoice, bool detectedTampering, GeminiComprehensiveResponse response)
        {
            try
            {
                // Create a validation result to populate with fraud detection
                var result = new ValidationResult { ExtractedInvoice = invoice };
                
                // Use enhanced fraud detection
                bool possibleFraud = await _fraudDetectionService.EnhancedFraudDetectionAsync(invoice, result);
                
                // Store raw response if available
                if (!string.IsNullOrEmpty(result.RawGeminiResponse))
                {
                    response.FraudDetectionResponse = result.RawGeminiResponse;
                }
                
                // Map validation result to comprehensive response
                MapValidationResultToComprehensiveResponse(result, response);
                
                // Set fraud properties
                response.PossibleFraud = possibleFraud;
                
                if (result.FraudDetection != null)
                {
                    response.FraudRiskLevel = result.FraudDetection.RiskLevel switch
                    {
                        >= 70 => "High",
                        >= 30 => "Medium",
                        _ => "Low"
                    };
                    
                    response.FraudDetectionConfidence = result.FraudDetection.RiskLevel / 100.0;
                    response.FraudAssessmentExplanation = result.FraudDetection.Notes;
                    response.RecommendedAction = result.FraudDetection.RecommendedAction;
                    
                    // Add recommended actions from fraud findings
                    if (result.FraudDetection.DetectedIndicators != null)
                    {
                        foreach (var indicator in result.FraudDetection.DetectedIndicators)
                        {
                            if (!string.IsNullOrEmpty(indicator.Reference) && 
                                !response.FraudRecommendedActions.Contains(indicator.Reference))
                            {
                                response.FraudRecommendedActions.Add(indicator.Reference);
                            }
                        }
                    }
                }
                
                _logger.LogInformation("Fraud detection completed: PossibleFraud={PossibleFraud}, RiskLevel={RiskLevel}", 
                    possibleFraud, response.FraudRiskLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in fraud detection");
                response.AddValidationIssue(ValidationSeverity.Error, 
                    $"Fraud detection failed: {ex.Message}");
            }
        }
        
        private async Task GetAuditAssessmentAsync(Invoice invoice, GeminiComprehensiveResponse response)
        {
            try
            {
                var result = await _lineItemService.GetAuditReadyAssessmentAsync(invoice);
                
                // Store raw response
                response.AuditAssessmentResponse = result.RawGeminiResponse;
                
                // Map validation result to comprehensive response
                MapValidationResultToComprehensiveResponse(result, response);
                
                // Use audit summary for detailed reasoning if not already set
                // Use ExecutiveSummary instead of Summary
                if (string.IsNullOrEmpty(response.DetailedReasoning) && result.AuditReport != null &&
                    !string.IsNullOrEmpty(result.AuditReport.ExecutiveSummary))
                {
                    response.DetailedReasoning = result.AuditReport.ExecutiveSummary;
                }

                _logger.LogInformation("Audit assessment completed: IsValid={IsValid}", result.IsValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in audit assessment");
                response.AddValidationIssue(ValidationSeverity.Error, 
                    $"Audit assessment failed: {ex.Message}");
            }
        }

        #endregion
    }
}
