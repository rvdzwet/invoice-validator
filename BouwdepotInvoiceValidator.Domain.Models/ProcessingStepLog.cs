namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Log entry for a processing step
    /// </summary>
    public class ProcessingStepLog
    {
        /// <summary>
        /// Name of the processing step
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Description of the step (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status of the step
        /// </summary>
        public ProcessingStepStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the step was performed
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Duration of the step in milliseconds
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Additional details about the step
        /// </summary>
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
    }
}
