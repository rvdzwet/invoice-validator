namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Result of applying a validation rule
    /// </summary>
    public class RuleValidationResult
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Description of the rule (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Result of the rule validation
        /// </summary>
        public RuleResult Result { get; set; }

        /// <summary>
        /// Explanation for the result (in DetectedLanguage)
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// Severity of the rule
        /// </summary>
        public RuleSeverity Severity { get; set; }

        /// <summary>
        /// Additional details about the rule validation
        /// </summary>
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
    }
}
