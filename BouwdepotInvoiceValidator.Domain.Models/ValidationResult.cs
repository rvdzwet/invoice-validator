namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Simplified response model for invoice validation API
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Unique identifier for this validation
        /// </summary>
        public string ValidationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp when the validation was performed
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Overall validation status
        /// </summary>
        public ValidationStatus Status { get; set; } = ValidationStatus.Unknown;

        /// <summary>
        /// Summary of the validation result
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Extracted invoice data
        /// </summary>
        public InvoiceData Invoice { get; set; } = new InvoiceData();

        /// <summary>
        /// List of validation issues
        /// </summary>
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();

        /// <summary>
        /// List of fraud indicators
        /// </summary>
        public List<FraudIndicator> FraudIndicators { get; set; } = new List<FraudIndicator>();

        /// <summary>
        /// Fraud risk assessment
        /// </summary>
        public FraudRiskAssessment FraudRisk { get; set; } = new FraudRiskAssessment();

        /// <summary>
        /// Bouwdepot eligibility assessment
        /// </summary>
        public BouwdepotEligibility BouwdepotEligibility { get; set; } = new BouwdepotEligibility();

        /// <summary>
        /// Processing information
        /// </summary>
        public ProcessingInfo Processing { get; set; } = new ProcessingInfo();
    }
}
