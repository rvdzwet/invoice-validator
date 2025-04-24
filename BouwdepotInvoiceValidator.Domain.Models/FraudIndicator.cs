namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Fraud indicator
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Category of the fraud indicator
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Description of the indicator
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Confidence score for this indicator (0.0-1.0)
        /// </summary>
        public double? Confidence { get; set; }
    }
}
