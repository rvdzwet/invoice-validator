namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Log entry for an AI model usage
    /// </summary>
    public class AIModelUsage
    {
        /// <summary>
        /// Name of the AI model
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Version of the AI model
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Operation performed with the model
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Number of tokens used
        /// </summary>
        public int TokenCount { get; set; }

        /// <summary>
        /// Timestamp when the model was used
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}
