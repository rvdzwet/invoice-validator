namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Log entry for a processing issue
    /// </summary>
    public class ProcessingIssue
    {
        /// <summary>
        /// Type of issue
        /// </summary>
        public string IssueType { get; set; }

        /// <summary>
        /// Description of the issue (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity of the issue
        /// </summary>
        public IssueSeverity Severity { get; set; }

        /// <summary>
        /// Timestamp when the issue occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Stack trace if available
        /// </summary>
        public string StackTrace { get; set; }
    }
}
