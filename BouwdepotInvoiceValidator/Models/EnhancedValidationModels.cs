using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Detailed content summary of what was purchased and why
    /// </summary>
    public class ContentSummary
    {
        /// <summary>
        /// Detailed description of what was purchased
        /// </summary>
        public string PurchasedItems { get; set; } = string.Empty;
        
        /// <summary>
        /// Analysis of the likely purpose/project
        /// </summary>
        public string IntendedPurpose { get; set; } = string.Empty;
        
        /// <summary>
        /// How these items impact the property value
        /// </summary>
        public string PropertyImpact { get; set; } = string.Empty;
        
        /// <summary>
        /// Category of home improvement (e.g., Kitchen Renovation, Bathroom Remodel)
        /// </summary>
        public string ProjectCategory { get; set; } = string.Empty;
        
        /// <summary>
        /// Estimated scope of the project (Small repair, Major renovation, etc.)
        /// </summary>
        public string EstimatedProjectScope { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this is likely part of a larger project
        /// </summary>
        public bool LikelyPartOfLargerProject { get; set; }
        
        /// <summary>
        /// Project stage (planning, in progress, finishing)
        /// </summary>
        public string ProjectStage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// A factor that contributed to the confidence score
    /// </summary>
    public class ConfidenceFactor
    {
        /// <summary>
        /// Name of the factor
        /// </summary>
        public string FactorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Impact on the confidence score (-20 to +20)
        /// </summary>
        public int Impact { get; set; }
        
        /// <summary>
        /// Explanation of why this factor was considered
        /// </summary>
        public string Explanation { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Comprehensive fraud detection result
    /// </summary>
    [Obsolete("Use FraudDetection class instead")]
    public class FraudDetectionResult
    {
        /// <summary>
        /// Overall fraud risk score (0-100)
        /// </summary>
        public int FraudRiskScore { get; set; }
        
        /// <summary>
        /// Risk level assessment
        /// </summary>
        public FraudRiskLevel RiskLevel { get; set; }
        
        /// <summary>
        /// List of detected fraud indicators
        /// </summary>
        public List<FraudIndicator> DetectedIndicators { get; set; } = new List<FraudIndicator>();
        
        /// <summary>
        /// Assessment of document integrity
        /// </summary>
        public DocumentIntegrityAssessment DocumentIntegrity { get; set; } = new DocumentIntegrityAssessment();
        
        /// <summary>
        /// Assessment of invoice content
        /// </summary>
        public ContentAssessment ContentAssessment { get; set; } = new ContentAssessment();
        
        /// <summary>
        /// Recommended action based on fraud risk
        /// </summary>
        public string RecommendedAction { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether manual review is required
        /// </summary>
        public bool RequiresManualReview { get; set; }
        
        /// <summary>
        /// Suggested verification steps
        /// </summary>
        public List<string> SuggestedVerificationSteps { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Fraud risk level enumeration
    /// </summary>
    public enum FraudRiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Detailed fraud indicator with evidence and severity
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Unique identifier for the indicator
        /// </summary>
        public string IndicatorId { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the indicator
        /// </summary>
        public string IndicatorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed description of the indicator
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Severity of the indicator (0-1 scale)
        /// </summary>
        public double Severity { get; set; }
        
        /// <summary>
        /// Category of the fraud indicator
        /// </summary>
        public FraudIndicatorCategory Category { get; set; }
        
        /// <summary>
        /// Specific evidence supporting this indicator
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
        
        /// <summary>
        /// Elements affected by this fraud indicator
        /// </summary>
        public List<string> AffectedElements { get; set; } = new List<string>();
        
        /// <summary>
        /// Confidence level in this indicator (0-1 scale)
        /// </summary>
        public double Confidence { get; set; }
    }
    
    /// <summary>
    /// Categories of fraud indicators
    /// </summary>
    public enum FraudIndicatorCategory
    {
        DocumentManipulation, 
        ContentInconsistency,
        AnomalousPricing,
        VendorIssue,
        HistoricalPattern,
        DigitalArtifact,
        ContextualMismatch,
        BehavioralFlag
    }
    
    /// <summary>
    /// Assessment of document integrity
    /// </summary>
    public class DocumentIntegrityAssessment
    {
        public bool HasDigitalTampering { get; set; }
        public bool HasInconsistentFormatting { get; set; }
        public bool HasDisruptedTextFlow { get; set; }
        public bool HasMisalignedElements { get; set; }
        public bool HasFontInconsistencies { get; set; }
        public bool HasSuspiciousMetadata { get; set; }
        public List<string> DigitalAnalysisResults { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Assessment of invoice content
    /// </summary>
    public class ContentAssessment
    {
        public bool HasMissingElements { get; set; }
        public bool HasInconsistentDates { get; set; }
        public bool HasMathematicalErrors { get; set; }
        public bool HasSuspiciousLineItems { get; set; }
        public bool HasVagueDescriptions { get; set; }
        public bool HasAmbiguousServices { get; set; }
        public List<string> ContentIssues { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Insights about the vendor from profiling
    /// </summary>
    public class VendorInsights
    {
        /// <summary>
        /// Name of the vendor
        /// </summary>
        public string VendorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Business categories the vendor operates in
        /// </summary>
        public List<string> BusinessCategories { get; set; } = new List<string>();
        
        /// <summary>
        /// Number of invoices previously processed from this vendor
        /// </summary>
        public int InvoiceCount { get; set; }
        
        /// <summary>
        /// When the vendor was first seen
        /// </summary>
        public DateTime FirstSeen { get; set; }
        
        /// <summary>
        /// When the vendor was last seen
        /// </summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>
        /// Reliability score for the vendor (0-1)
        /// </summary>
        public double ReliabilityScore { get; set; }
        
        /// <summary>
        /// Price stability score (0-1)
        /// </summary>
        public double PriceStabilityScore { get; set; }
        
        /// <summary>
        /// Quality of document score (0-1)
        /// </summary>
        public double DocumentQualityScore { get; set; }
        
        /// <summary>
        /// Whether unusual services were detected
        /// </summary>
        public bool UnusualServicesDetected { get; set; }
        
        /// <summary>
        /// Whether unreasonable prices were detected
        /// </summary>
        public bool UnreasonablePricesDetected { get; set; }
        
        /// <summary>
        /// Total number of anomalies detected for this vendor
        /// </summary>
        public int TotalAnomalyCount { get; set; }
        
        /// <summary>
        /// Services this vendor specializes in
        /// </summary>
        public List<string> VendorSpecialties { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Comprehensive audit report for transparency
    /// </summary>
    public class AuditReport
    {
        /// <summary>
        /// Unique audit identifier
        /// </summary>
        public string AuditIdentifier { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// When the audit was created (UTC)
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Version of the AI model used
        /// </summary>
        public string ModelVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Version of the validation system
        /// </summary>
        public string ValidatorVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Cryptographic hash of the document
        /// </summary>
        public string DocumentHash { get; set; } = string.Empty;
        
        /// <summary>
        /// Source of the document (filename)
        /// </summary>
        public string DocumentSource { get; set; } = string.Empty;
        
        /// <summary>
        /// List of verification tests performed
        /// </summary>
        public List<string> VerificationTests { get; set; } = new List<string>();
        
        /// <summary>
        /// Executive summary of the decision
        /// </summary>
        public string ExecutiveSummary { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed assessments of each rule applied
        /// </summary>
        public List<RuleAssessment> RuleAssessments { get; set; } = new List<RuleAssessment>();
        
        /// <summary>
        /// References to applicable regulations
        /// </summary>
        public List<RegulationReference> ApplicableRegulations { get; set; } = new List<RegulationReference>();
        
        /// <summary>
        /// Key metrics about the validation
        /// </summary>
        public Dictionary<string, double> KeyMetrics { get; set; } = new Dictionary<string, double>();
        
        /// <summary>
        /// Threshold configuration applied in this validation
        /// </summary>
        public ValidationThresholdConfiguration AppliedThresholds { get; set; } = new ValidationThresholdConfiguration();
        
        /// <summary>
        /// Factors supporting approval
        /// </summary>
        public List<string> ApprovalFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Factors raising concerns
        /// </summary>
        public List<string> ConcernFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Trace of processing events
        /// </summary>
        public List<ProcessingEvent> ProcessingEvents { get; set; } = new List<ProcessingEvent>();
        
        /// <summary>
        /// Vendor insights for audit context
        /// </summary>
        public VendorInsights VendorInsights { get; set; } = new VendorInsights();
    }
    
    /// <summary>
    /// Assessment of a specific rule
    /// </summary>
    public class RuleAssessment
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the rule
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the rule is satisfied
        /// </summary>
        public bool IsSatisfied { get; set; }
        
        /// <summary>
        /// Evidence supporting the assessment
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed reasoning for the decision
        /// </summary>
        public string Reasoning { get; set; } = string.Empty;
        
        /// <summary>
        /// Weight assigned to this rule (importance)
        /// </summary>
        public int Weight { get; set; }
        
        /// <summary>
        /// Score for this rule (0-100)
        /// </summary>
        public int Score { get; set; }
    }
    
    /// <summary>
    /// Reference to an applicable regulation
    /// </summary>
    public class RegulationReference
    {
        /// <summary>
        /// Reference code for the regulation
        /// </summary>
        public string ReferenceCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the regulation
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Why this regulation applies
        /// </summary>
        public string ApplicabilityExplanation { get; set; } = string.Empty;
        
        /// <summary>
        /// Compliance status (Compliant/Non-compliant/Partially compliant)
        /// </summary>
        public string ComplianceStatus { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Record of a processing event in the validation pipeline
    /// </summary>
    public class ProcessingEvent
    {
        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Type of event
        /// </summary>
        public string EventType { get; set; } = string.Empty;
        
        /// <summary>
        /// Component that generated the event
        /// </summary>
        public string Component { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Parameters associated with the event
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Configuration for validation thresholds
    /// </summary>
    public class ValidationThresholdConfiguration
    {
        /// <summary>
        /// Threshold for automatic approval (0-100)
        /// </summary>
        public int AutoApprovalThreshold { get; set; } = 75;
        
        /// <summary>
        /// Threshold for high risk (0-100)
        /// </summary>
        public int HighRiskThreshold { get; set; } = 40;
        
        /// <summary>
        /// Whether auto-approval is enabled
        /// </summary>
        public bool EnableAutoApproval { get; set; } = true;
        
        /// <summary>
        /// When the configuration was last modified
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Fraud detection information
    /// </summary>
    public class FraudDetection
    {
        /// <summary>
        /// Overall fraud risk score (0-100)
        /// </summary>
        public int FraudRiskScore { get; set; } // Added missing property
        
        /// <summary>
        /// Risk level assessment
        /// </summary>
        public FraudRiskLevel RiskLevel { get; set; } = FraudRiskLevel.Low;
        
        /// <summary>
        /// List of detected fraud indicators
        /// </summary>
        public List<FraudIndicator> DetectedIndicators { get; set; } = new List<FraudIndicator>();
        
        /// <summary>
        /// Recommended action based on fraud risk
        /// </summary>
        public string RecommendedAction { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether manual review is required
        /// </summary>
        public bool RequiresManualReview { get; set; }
        
        /// <summary>
        /// Suggested verification steps
        /// </summary>
        public List<string> SuggestedVerificationSteps { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Digital signature for verification
    /// </summary>
    public class DigitalSignature
    {
        /// <summary>
        /// The actual signature value (Base64 encoded)
        /// </summary>
        public string SignatureValue { get; set; } = string.Empty;
        
        /// <summary>
        /// When the signature was created (UTC)
        /// </summary>
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Algorithm used for signing
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;
        
        /// <summary>
        /// Fields that were included in the signature
        /// </summary>
        public string SignedFields { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the entity that created the signature
        /// </summary>
        public string SignerId { get; set; } = string.Empty;
        
        /// <summary>
        /// Verification status of the signature
        /// </summary>
        public SignatureVerificationStatus VerificationStatus { get; set; } = SignatureVerificationStatus.NotVerified;
    }
    
    /// <summary>
    /// Status of digital signature verification
    /// </summary>
    public enum SignatureVerificationStatus
    {
        /// <summary>
        /// Signature has not been verified
        /// </summary>
        NotVerified,
        
        /// <summary>
        /// Signature verification was successful
        /// </summary>
        Valid,
        
        /// <summary>
        /// Signature verification failed
        /// </summary>
        Invalid,
        
        /// <summary>
        /// Signature verification failed due to crypto error
        /// </summary>
        CryptoError
    }
}
