namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Result of fraud analysis
    /// </summary>
    public class FraudAnalysisResult
    {
        /// <summary>
        /// Risk level assessment
        /// </summary>
        public string RiskLevel { get; set; }

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public int? RiskScore { get; set; }

        /// <summary>
        /// Summary of findings (in DetectedLanguage)
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detected fraud indicators
        /// </summary>
        public List<FraudIndicator> Indicators { get; set; } = new List<FraudIndicator>();
    }
}
