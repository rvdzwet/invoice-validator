using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Record of a processing event in the validation pipeline
    /// </summary>
    public class ProcessingEvent
    {
        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Type of event
        /// </summary>
        public string EventType { get; set; } = string.Empty;
        
        /// <summary>
        /// Component that generated the event
        /// </summary>
        public string Component { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Parameters associated with the event
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
