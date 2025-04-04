using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Technical details about the validation process
    /// </summary>
    public class TechnicalDetails
    {
        public string ValidationEngine { get; set; } = string.Empty;
        public string EngineVersion { get; set; } = string.Empty;
        public DateTime ProcessingStartTime { get; set; }
        public DateTime ProcessingEndTime { get; set; }
        public int ProcessingDurationMs { get; set; }
        public Dictionary<string, string> SystemParameters { get; set; } = new Dictionary<string, string>();
        public List<string> AppliedModels { get; set; } = new List<string>();
        public Dictionary<string, double> ModelConfidenceScores { get; set; } = new Dictionary<string, double>();
    }
}
