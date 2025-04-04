using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Record of a processing step in the validation pipeline
    /// </summary>
    public class ProcessingStep
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessorIdentifier { get; set; } = string.Empty;
        public string OutputState { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public string InputHash { get; set; } = string.Empty;
        public string OutputHash { get; set; } = string.Empty;
    }
}
