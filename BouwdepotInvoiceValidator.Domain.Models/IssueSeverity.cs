namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Severity of a validation issue
    /// </summary>
    public enum IssueSeverity
    {
        /// <summary>
        /// Informational message
        /// </summary>
        Info,

        /// <summary>
        /// Warning message
        /// </summary>
        Warning,

        /// <summary>
        /// Error message
        /// </summary>
        Error,

        /// <summary>
        /// Critical error
        /// </summary>
        Critical
    }
}
