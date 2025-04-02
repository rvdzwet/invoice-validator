using System;
using System.Collections.Generic;
using System.Drawing;

namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Detailed summary of invoice content and purpose
    /// </summary>
    public class ContentSummary
    {
        /// <summary>
        /// Summary of what was purchased
        /// </summary>
        public string PurchasedItems { get; set; } = string.Empty;
        
        /// <summary>
        /// What these items are likely used for
        /// </summary>
        public string IntendedPurpose { get; set; } = string.Empty;
        
        /// <summary>
        /// How this affects property value or improvement
        /// </summary>
        public string PropertyImpact { get; set; } = string.Empty;
        
        /// <summary>
        /// Category of home improvement
        /// </summary>
        public string ProjectCategory { get; set; } = string.Empty;
        
        /// <summary>
        /// Small repair, major renovation, etc.
        /// </summary>
        public string EstimatedProjectScope { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this is likely part of a larger project
        /// </summary>
        public bool LikelyPartOfLargerProject { get; set; }
        
        /// <summary>
        /// Initial, ongoing, finishing touches, etc.
        /// </summary>
        public string ProjectStage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Information about what was purchased based on line item analysis
    /// </summary>
    public class PurchaseAnalysis
    {
        public string Summary { get; set; } = string.Empty;
        public string PrimaryPurpose { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new List<string>();
        public double HomeImprovementPercentage { get; set; }
        public List<LineItemAnalysisDetail> LineItemDetails { get; set; } = new List<LineItemAnalysisDetail>();
    }
    
    /// <summary>
    /// Detailed analysis of a single line item from an invoice
    /// </summary>
    public class LineItemAnalysisDetail
    {
        public string Description { get; set; } = string.Empty;
        public string InterpretedAs { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsHomeImprovement { get; set; }
        public double Confidence { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Analysis of a single line item and its relevance to home improvement
    /// </summary>
    public class LineItemAnalysis
    {
        /// <summary>
        /// Description of the line item as extracted from invoice
        /// </summary>
        public string ItemDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// How relevant this item is to home improvement (0.0-1.0)
        /// </summary>
        public double HomeImprovementRelevance { get; set; }
        
        /// <summary>
        /// Category of the product or service
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Likely purpose of this item in home improvement context
        /// </summary>
        public string Purpose { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional observations about this line item
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Represents the result of an invoice validation process
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Unique identifier for the validation result
        /// </summary>
        public string ValidationId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Whether the invoice is valid overall
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// List of validation issues found
        /// </summary>
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
        
        /// <summary>
        /// The extracted invoice data
        /// </summary>
        public Invoice? ExtractedInvoice { get; set; }
        
        /// <summary>
        /// Whether the document shows signs of tampering
        /// </summary>
        public bool PossibleTampering { get; set; }
        
        /// <summary>
        /// Whether the invoice is for home improvement
        /// </summary>
        public bool IsHomeImprovement { get; set; }
        
        /// <summary>
        /// Raw response from AI provider (for debugging)
        /// </summary>
        public string RawAIResponse { get; set; } = string.Empty;
        
        /// <summary>
        /// Raw Gemini response (for backward compatibility)
        /// </summary>
        [Obsolete("Use RawAIResponse instead")]
        public string RawGeminiResponse 
        { 
            get => RawAIResponse; 
            set => RawAIResponse = value; 
        }
        
        /// <summary>
        /// Confidence score for the validation (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Whether the confidence score meets the auto-approval threshold
        /// </summary>
        public bool MeetsApprovalThreshold { get; set; }
        
        /// <summary>
        /// ID of the claimant (for behavioral fraud analysis)
        /// </summary>
        public string ClaimantId { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp when validation was performed
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Content summary - detailed analysis of what was purchased and why
        /// </summary>
        public ContentSummary ContentSummary { get; set; } = new ContentSummary();
        
        /// <summary>
        /// Factors that contributed to the confidence score
        /// </summary>
        public List<ConfidenceFactor> ConfidenceFactors { get; set; } = new List<ConfidenceFactor>();
        
        /// <summary>
        /// Bouwdepot-specific validation properties
        /// </summary>
        public bool IsBouwdepotCompliant { get; set; }
        
        /// <summary>
        /// Whether the invoice qualifies for Verduurzamingsdepot
        /// </summary>
        public bool IsVerduurzamingsdepotCompliant { get; set; }
        
        /// <summary>
        /// Detailed Bouwdepot validation information
        /// </summary>
        public BouwdepotValidationDetails BouwdepotValidation { get; set; } = new BouwdepotValidationDetails();
        
        /// <summary>
        /// Document verification properties - whether this is a verified invoice document
        /// </summary>
        public bool? IsVerifiedInvoice { get; set; }
        
        /// <summary>
        /// Detected document type
        /// </summary>
        public string DetectedDocumentType { get; set; } = string.Empty;
        
        /// <summary>
        /// Confidence in the document verification
        /// </summary>
        public double DocumentVerificationConfidence { get; set; }
        
        /// <summary>
        /// Invoice elements present in the document
        /// </summary>
        public List<string> PresentInvoiceElements { get; set; } = new List<string>();
        
        /// <summary>
        /// Invoice elements missing from the document
        /// </summary>
        public List<string> MissingInvoiceElements { get; set; } = new List<string>();
        
        /// <summary>
        /// Fraud detection results
        /// </summary>
        public FraudDetection FraudDetection { get; set; } = new FraudDetection();
        
        /// <summary>
        /// Vendor insights from profiling
        /// </summary>
        public VendorInsights VendorInsights { get; set; } = new VendorInsights();
        
        /// <summary>
        /// Audit report for transparency
        /// </summary>
        public AuditReport AuditReport { get; set; } = new AuditReport();
        
        /// <summary>
        /// Digital signature for verification
        /// </summary>
        public DigitalSignature Signature { get; set; } = new DigitalSignature();
        
        /// <summary>
        /// Purchase analysis information
        /// </summary>
        public PurchaseAnalysis PurchaseAnalysis { get; set; } = new PurchaseAnalysis();
        
        /// <summary>
        /// Analysis of individual line items in the invoice
        /// </summary>
        public List<LineItemAnalysisDetail> LineItemAnalysis { get; set; } = new List<LineItemAnalysisDetail>();
        
        /// <summary>
        /// Detailed reasoning from the AI model providing comprehensive explanation of all assessments
        /// </summary>
        public string DetailedReasoning { get; set; } = string.Empty;
        
        #region Backward Compatibility Properties
        
        /// <summary>
        /// Weighted score for backward compatibility
        /// </summary>
        [Obsolete("Use ConfidenceScore instead")]
        public int WeightedScore { get => ConfidenceScore; set => ConfidenceScore = value; }
        
        /// <summary>
        /// Audit justification for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport.ExecutiveSummary instead")]
        public string AuditJustification { get; set; } = string.Empty;
        
        /// <summary>
        /// Criteria assessments for backward compatibility
        /// </summary>
        [Obsolete("Use dedicated assessment classes instead")]
        public List<CriterionAssessment> CriteriaAssessments { get; set; } = new List<CriterionAssessment>();
        
        /// <summary>
        /// Visual assessments for backward compatibility
        /// </summary>
        [Obsolete("Use dedicated assessment classes instead")]
        public List<VisualAssessment> VisualAssessments { get; set; } = new List<VisualAssessment>();
        
        /// <summary>
        /// Regulatory notes for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport.ApplicableRegulations instead")]
        public List<string> RegulatoryNotes { get; set; } = new List<string>();
        
        /// <summary>
        /// Approval factors for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport.ApprovalFactors instead")]
        public List<string> ApprovalFactors 
        { 
            get => AuditReport?.ApprovalFactors ?? new List<string>(); 
            set { if (AuditReport != null) AuditReport.ApprovalFactors = value; } 
        }
        
        /// <summary>
        /// Rejection factors for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport.ConcernFactors instead")]
        public List<string> RejectionFactors 
        { 
            get => AuditReport?.ConcernFactors ?? new List<string>(); 
            set { if (AuditReport != null) AuditReport.ConcernFactors = value; } 
        }
        
        /// <summary>
        /// Met rules for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport.RuleAssessments instead")]
        public List<string> MetRules { get; set; } = new List<string>();
        
        /// <summary>
        /// Violated rules for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport.RuleAssessments instead")]
        public List<string> ViolatedRules { get; set; } = new List<string>();
        
        /// <summary>
        /// Recommended actions for backward compatibility
        /// </summary>
        [Obsolete("Use FraudDetection.RecommendedAction instead")]
        public List<string> RecommendedActions { get; set; } = new List<string>();
        
        /// <summary>
        /// Audit info for backward compatibility
        /// </summary>
        [Obsolete("Use AuditReport instead")]
        public AuditMetadata AuditInfo { get; set; } = new AuditMetadata();
        
        /// <summary>
        /// Fraud indicator assessments for backward compatibility
        /// </summary>
        [Obsolete("Use FraudDetection.DetectedIndicators instead")]
        public List<IndicatorAssessment> FraudIndicatorAssessments { get; set; } = new List<IndicatorAssessment>();
        
        #endregion
        
        /// <summary>
        /// Add an issue to the validation result
        /// </summary>
        /// <param name="severity">The severity of the issue</param>
        /// <param name="message">The issue message</param>
        public void AddIssue(ValidationSeverity severity, string message)
        {
            Issues.Add(new ValidationIssue
            {
                Severity = severity,
                Message = message
            });
            
            // Update IsValid based on issue severities
            if (severity == ValidationSeverity.Error)
            {
                IsValid = false;
            }
        }
        
        /// <summary>
        /// Add a confidence factor to the validation result
        /// </summary>
        public void AddConfidenceFactor(string factorName, int impact, string explanation)
        {
            ConfidenceFactors.Add(new ConfidenceFactor
            {
                FactorName = factorName,
                Impact = impact,
                Explanation = explanation
            });
        }
    }

    /// <summary>
    /// Represents an issue found during validation
    /// </summary>
    public class ValidationIssue
    {
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enum representing the severity of a validation issue
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
    
    /// <summary>
    /// Metadata for audit trails
    /// </summary>
    public class AuditMetadata
    {
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string FrameworkVersion { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Assessment of a specific validation criterion
    /// </summary>
    public class CriterionAssessment
    {
        public string CriterionName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Evidence { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    /// <summary>
    /// Assessment of a specific fraud indicator
    /// </summary>
    public class IndicatorAssessment
    {
        public string IndicatorName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Evidence { get; set; } = string.Empty;
        public int ConcernLevel { get; set; }
        public string RegulatoryReference { get; set; } = string.Empty;
        public int Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Assessment of a visual element in the invoice
    /// </summary>
    public class VisualAssessment
    {
        public string ElementName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Evidence { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public RectangleF? Location { get; set; }
    }
    
    /// <summary>
    /// Represents a factor that contributes to the confidence score
    /// </summary>
    public class ConfidenceFactor
    {
        /// <summary>
        /// Name of the factor affecting confidence
        /// </summary>
        public string FactorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Impact on confidence score (-10 to +10)
        /// </summary>
        public int Impact { get; set; }
        
        /// <summary>
        /// Explanation of this factor's impact on the overall assessment
        /// </summary>
        public string Explanation { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Fraud detection results for the invoice validation
    /// </summary>
    public class FraudDetection
    {
        /// <summary>
        /// Overall fraud risk level (0-100)
        /// </summary>
        public int RiskLevel { get; set; }
        
        /// <summary>
        /// Whether any fraud indicators were detected
        /// </summary>
        public bool FraudIndicatorsDetected { get; set; }
        
        /// <summary>
        /// List of detected fraud indicators with details
        /// </summary>
        public List<FraudIndicator> DetectedIndicators { get; set; } = new List<FraudIndicator>();
        
        /// <summary>
        /// Recommended action based on fraud assessment
        /// </summary>
        public string RecommendedAction { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional information about the fraud assessment
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// A specific indicator of potential fraud in the invoice
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Name of the fraud indicator
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Severity of this indicator (0-100)
        /// </summary>
        public int Severity { get; set; }
        
        /// <summary>
        /// Evidence supporting this indicator
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
        
        /// <summary>
        /// Regulatory or policy reference
        /// </summary>
        public string Reference { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Insights about the vendor based on profiling
    /// </summary>
    public class VendorInsights
    {
        /// <summary>
        /// Vendor name from the invoice
        /// </summary>
        public string VendorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this vendor is known from previous invoices
        /// </summary>
        public bool IsKnownVendor { get; set; }
        
        /// <summary>
        /// Type of vendor (Contractor, Supplier, Service Provider, etc.)
        /// </summary>
        public string VendorType { get; set; } = string.Empty;
        
        /// <summary>
        /// Primary business areas of this vendor
        /// </summary>
        public List<string> BusinessAreas { get; set; } = new List<string>();
        
        /// <summary>
        /// Reputation assessment of this vendor
        /// </summary>
        public string ReputationAssessment { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional insights about the vendor
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Detailed audit report for the validation
    /// </summary>
    public class AuditReport
    {
        /// <summary>
        /// Executive summary of the validation
        /// </summary>
        public string ExecutiveSummary { get; set; } = string.Empty;
        
        /// <summary>
        /// List of key observations from the validation
        /// </summary>
        public List<string> KeyObservations { get; set; } = new List<string>();
        
        /// <summary>
        /// Factors supporting approval
        /// </summary>
        public List<string> ApprovalFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Factors raising concerns
        /// </summary>
        public List<string> ConcernFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Applicable regulations and policies
        /// </summary>
        public List<string> ApplicableRegulations { get; set; } = new List<string>();
        
        /// <summary>
        /// Assessments of specific rules
        /// </summary>
        public List<RuleAssessment> RuleAssessments { get; set; } = new List<RuleAssessment>();
    }
    
    /// <summary>
    /// Assessment of a specific rule
    /// </summary>
    public class RuleAssessment
    {
        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the rule
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the rule was met
        /// </summary>
        public bool IsMet { get; set; }
        
        /// <summary>
        /// Evidence supporting the assessment
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
        
        /// <summary>
        /// Impact of this rule on the overall validation
        /// </summary>
        public string Impact { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Digital signature for verification
    /// </summary>
    public class DigitalSignature
    {
        /// <summary>
        /// Timestamp when the signature was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Algorithm used for the signature
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;
        
        /// <summary>
        /// Base64 encoded signature value
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Identifier for the public key used for verification
        /// </summary>
        public string KeyId { get; set; } = string.Empty;
    }
}
