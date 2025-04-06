namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Severity of a rule
    /// </summary>
    public enum RuleSeverity
    {
        /// <summary>
        /// Informational rule
        /// </summary>
        Info,

        /// <summary>
        /// Warning rule
        /// </summary>
        Warning,

        /// <summary>
        /// Error rule
        /// </summary>
        Error,

        /// <summary>
        /// Critical rule
        /// </summary>
        Critical
    }
}
