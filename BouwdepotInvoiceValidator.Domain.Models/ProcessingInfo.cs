namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Processing information
    /// </summary>
    public class ProcessingInfo
    {
        /// <summary>
        /// Duration of the processing in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// AI models used during processing
        /// </summary>
        public List<string> ModelsUsed { get; set; } = new List<string>();

        /// <summary>
        /// Processing steps performed
        /// </summary>
        public List<string> Steps { get; set; } = new List<string>();
    }
}
