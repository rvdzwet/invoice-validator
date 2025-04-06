namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Fraud risk assessment
    /// </summary>
    public class FraudRiskAssessment
    {
        /// <summary>
        /// Risk level assessment
        /// </summary>
        public FraudRiskLevel RiskLevel { get; set; } = FraudRiskLevel.Unknown;

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public int? RiskScore { get; set; }

        /// <summary>
        /// Summary of findings
        /// </summary>
        public string Summary { get; set; }
    }
}
