using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
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
}
