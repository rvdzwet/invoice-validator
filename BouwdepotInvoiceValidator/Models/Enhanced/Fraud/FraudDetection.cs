using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Fraud detection information
    /// </summary>
    public class FraudDetection
    {
        /// <summary>
        /// Overall fraud risk score (0-100)
        /// </summary>
        public int FraudRiskScore { get; set; }
        
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
}
