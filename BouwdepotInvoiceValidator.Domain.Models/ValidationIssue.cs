namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Validation issue
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Type of issue
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Description of the issue
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity of the issue
        /// </summary>
        public IssueSeverity Severity { get; set; }

        /// <summary>
        /// Field or section that the issue relates to
        /// </summary>
        public string Field { get; set; }
    }
}
