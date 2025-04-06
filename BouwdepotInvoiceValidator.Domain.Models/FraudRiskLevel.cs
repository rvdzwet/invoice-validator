namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Fraud risk level
    /// </summary>
    public enum FraudRiskLevel
    {
        /// <summary>
        /// Risk level has not been assessed
        /// </summary>
        Unknown,

        /// <summary>
        /// Low risk of fraud
        /// </summary>
        Low,

        /// <summary>
        /// Medium risk of fraud
        /// </summary>
        Medium,

        /// <summary>
        /// High risk of fraud
        /// </summary>
        High,

        /// <summary>
        /// Very high risk of fraud
        /// </summary>
        VeryHigh
    }
}
