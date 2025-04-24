namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Result of a rule validation
    /// </summary>
    public enum RuleResult
    {
        /// <summary>
        /// The rule passed
        /// </summary>
        Pass,

        /// <summary>
        /// The rule failed
        /// </summary>
        Fail,

        /// <summary>
        /// The rule was not applicable
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The rule passed but with warnings
        /// </summary>
        Warning
    }
}
