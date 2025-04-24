using System.Drawing;
using BouwdepotInvoiceValidator.Models.Enhanced; // Using statement already added
// using BouwdepotInvoiceValidator.Models.Analysis; // Remove this using statement

namespace BouwdepotInvoiceValidator.Domain.Models
{
    // Removed duplicate ContentSummary class definition (exists in Enhanced)

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
    /// Detailed analysis of a single line item from an invoice (Restored definition)
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
        /// Fraud detection results (Using Enhanced model)
        /// </summary>
        public BouwdepotInvoiceValidator.Models.Enhanced.FraudDetection FraudDetection { get; set; } = new BouwdepotInvoiceValidator.Models.Enhanced.FraudDetection();
        
        /// <summary>
        /// Vendor insights from profiling (Using Enhanced model)
        /// </summary>
        public VendorInsights VendorInsights { get; set; } = new VendorInsights();
        
        /// <summary>
        /// Audit report for transparency (Using Enhanced model)
        /// </summary>
        public BouwdepotInvoiceValidator.Models.Enhanced.AuditReport AuditReport { get; set; } = new BouwdepotInvoiceValidator.Models.Enhanced.AuditReport();
        
        /// <summary>
        /// Digital signature for verification (Using Enhanced model)
        /// </summary>
        public BouwdepotInvoiceValidator.Models.Enhanced.DigitalSignature Signature { get; set; } = new BouwdepotInvoiceValidator.Models.Enhanced.DigitalSignature();
        
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

        /// <summary>
        /// Summary of the invoice content and purpose. (Using Enhanced model)
        /// </summary>
        public BouwdepotInvoiceValidator.Models.Enhanced.ContentSummary Summary { get; set; } = new BouwdepotInvoiceValidator.Models.Enhanced.ContentSummary();

        /// <summary>
        /// Recommendation from the audit assessment (e.g., Approve, Reject, Review)
        /// </summary>
        public string AuditRecommendation { get; set; } = string.Empty;
        
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
    /// Assessment of a specific fraud indicator (Restored definition)
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
    
    // Removed duplicate ConfidenceFactor class definition (assuming it exists elsewhere or is not needed here)
    
    // Removed duplicate FraudDetection class definition (exists in Enhanced)
    
    // Removed duplicate FraudIndicator class definition (exists in Enhanced)
    
    // Removed duplicate VendorInsights class definition (exists in Enhanced)
    
    // Removed duplicate AuditReport class definition (exists in Enhanced)
    
    // Removed duplicate RuleAssessment class definition (exists in Enhanced)
    
    // Removed duplicate DigitalSignature class definition (exists in Enhanced)
}
