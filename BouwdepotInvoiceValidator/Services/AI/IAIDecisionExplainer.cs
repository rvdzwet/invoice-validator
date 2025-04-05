using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Interface for services that explain AI decisions for transparency and assurance
    /// </summary>
    public interface IAIDecisionExplainer
    {
        /// <summary>
        /// Generates a detailed explanation of AI validation decisions
        /// </summary>
        Task<AIDecisionExplanation> ExplainValidationResultAsync(ValidationResult validationResult);
        
        /// <summary>
        /// Generates a detailed explanation of vendor verification
        /// </summary>
        Task<AIDecisionExplanation> ExplainVendorVerificationAsync(string vendorName, VendorVerificationResult verification);
        
        /// <summary>
        /// Explains the reasoning behind specific validation scores
        /// </summary>
        Task<AIDecisionExplanation> ExplainValidationScoreAsync(string scoreType, int score, List<string> factors);
        
        /// <summary>
        /// Analyzes the confidence thresholds and margins of error
        /// </summary>
        Task<ConfidenceAnalysis> AnalyzeConfidenceAsync(ValidationResult validationResult);
    }
    
    /// <summary>
    /// Detailed explanation of an AI decision
    /// </summary>
    public class AIDecisionExplanation
    {
        /// <summary>
        /// Summary of the decision suitable for display to users (non-technical)
        /// </summary>
        public string SummaryExplanation { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed explanation with all reasoning steps
        /// </summary>
        public string DetailedExplanation { get; set; } = string.Empty;
        
        /// <summary>
        /// Technical explanation with model details and confidence metrics
        /// </summary>
        public string TechnicalExplanation { get; set; } = string.Empty;
        
        /// <summary>
        /// List of decision factors and their weights
        /// </summary>
        public List<DecisionFactor> Factors { get; set; } = new List<DecisionFactor>();
        
        /// <summary>
        /// List of evidence pieces used to make the decision
        /// </summary>
        public List<EvidenceItem> Evidence { get; set; } = new List<EvidenceItem>();
        
        /// <summary>
        /// Alternate interpretations that were considered
        /// </summary>
        public List<AlternateInterpretation> AlternateInterpretations { get; set; } = new List<AlternateInterpretation>();
        
        /// <summary>
        /// References to specific applicable validation rules
        /// </summary>
        public List<RuleReference> ApplicableRules { get; set; } = new List<RuleReference>();
        
        /// <summary>
        /// Potential weaknesses in the analysis
        /// </summary>
        public List<string> PotentialWeakPoints { get; set; } = new List<string>();
        
        /// <summary>
        /// Confidence metrics for this decision
        /// </summary>
        public ConfidenceMetrics Confidence { get; set; } = new ConfidenceMetrics();
    }
    
    /// <summary>
    /// A single factor influencing an AI decision
    /// </summary>
    public class DecisionFactor
    {
        /// <summary>
        /// Name of the factor
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Weight or importance of this factor (0-100)
        /// </summary>
        public int Weight { get; set; }
        
        /// <summary>
        /// Description of how this factor influenced the decision
        /// </summary>
        public string Impact { get; set; } = string.Empty;
        
        /// <summary>
        /// Value or assessment of this factor
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Evidence used for making a decision
    /// </summary>
    public class EvidenceItem
    {
        /// <summary>
        /// Type of evidence (text, image, etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Location of the evidence in the document
        /// </summary>
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// The actual evidence content
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Relevance or weight of this evidence (0-100)
        /// </summary>
        public int Relevance { get; set; }
        
        /// <summary>
        /// Coordinates for visual evidence (X, Y, Width, Height)
        /// </summary>
        public int[] Coordinates { get; set; } = Array.Empty<int>();
    }
    
    /// <summary>
    /// An alternative interpretation that was considered but rejected
    /// </summary>
    public class AlternateInterpretation
    {
        /// <summary>
        /// Description of the alternate interpretation
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Confidence score for this interpretation (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Reasons why this interpretation was rejected
        /// </summary>
        public List<string> RejectionReasons { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Reference to a specific validation rule
    /// </summary>
    public class RuleReference
    {
        /// <summary>
        /// Identifier for the rule
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
        /// Whether the rule was satisfied
        /// </summary>
        public bool IsSatisfied { get; set; }
        
        /// <summary>
        /// Evidence for rule satisfaction/violation
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Confidence metrics for the decision
    /// </summary>
    public class ConfidenceMetrics
    {
        /// <summary>
        /// Overall confidence score (0-100)
        /// </summary>
        public int OverallScore { get; set; }
        
        /// <summary>
        /// Margin of error (percentage points)
        /// </summary>
        public double MarginOfError { get; set; }
        
        /// <summary>
        /// Minimum confidence threshold for a positive decision
        /// </summary>
        public int ApprovalThreshold { get; set; }
        
        /// <summary>
        /// Statistical confidence level (e.g., 95%, 99%)
        /// </summary>
        public double ConfidenceLevel { get; set; }
    }
    
    /// <summary>
    /// Detailed confidence analysis for a validation result
    /// </summary>
    public class ConfidenceAnalysis
    {
        /// <summary>
        /// Overall confidence score
        /// </summary>
        public int OverallConfidence { get; set; }
        
        /// <summary>
        /// Confidence intervals for different aspects
        /// </summary>
        public Dictionary<string, ConfidenceInterval> ConfidenceIntervals { get; set; } = new Dictionary<string, ConfidenceInterval>();
        
        /// <summary>
        /// Statistical distribution of confidence
        /// </summary>
        public string DistributionType { get; set; } = string.Empty;
        
        /// <summary>
        /// Explanation of the confidence calculation
        /// </summary>
        public string ConfidenceMethodology { get; set; } = string.Empty;
        
        /// <summary>
        /// Factors that could increase confidence
        /// </summary>
        public List<string> ConfidenceIncreasingFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Factors that decrease confidence
        /// </summary>
        public List<string> ConfidenceDecreasingFactors { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Confidence interval for a specific aspect
    /// </summary>
    public class ConfidenceInterval
    {
        /// <summary>
        /// Lower bound of the confidence interval
        /// </summary>
        public double LowerBound { get; set; }
        
        /// <summary>
        /// Upper bound of the confidence interval
        /// </summary>
        public double UpperBound { get; set; }
        
        /// <summary>
        /// Confidence level (e.g., 95%, 99%)
        /// </summary>
        public double ConfidenceLevel { get; set; }
    }
    
    /// <summary>
    /// Result of vendor verification
    /// </summary>
    public class VendorVerificationResult
    {
        /// <summary>
        /// Whether the vendor is verified
        /// </summary>
        public bool IsVerified { get; set; }
        
        /// <summary>
        /// Confidence in the verification (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Sources used for verification
        /// </summary>
        public List<string> VerificationSources { get; set; } = new List<string>();
        
        /// <summary>
        /// Risk profile for this vendor
        /// </summary>
        public VendorRiskProfile RiskProfile { get; set; } = new VendorRiskProfile();
        
        /// <summary>
        /// Detected anomalies in vendor information
        /// </summary>
        public List<string> DetectedAnomalies { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Risk profile for a vendor
    /// </summary>
    public class VendorRiskProfile
    {
        /// <summary>
        /// Overall risk level (Low, Medium, High)
        /// </summary>
        public string RiskLevel { get; set; } = "Low";
        
        /// <summary>
        /// Numerical risk score (0-100)
        /// </summary>
        public int RiskScore { get; set; }
        
        /// <summary>
        /// Risk factors identified
        /// </summary>
        public List<string> RiskFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Mitigating factors that reduce risk
        /// </summary>
        public List<string> MitigatingFactors { get; set; } = new List<string>();
    }
}
