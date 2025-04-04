using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Detailed fraud indicator with evidence and severity
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Unique identifier for the indicator
        /// </summary>
        public string IndicatorId { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the indicator
        /// </summary>
        public string IndicatorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed description of the indicator
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Severity of the indicator (0-1 scale)
        /// </summary>
        public double Severity { get; set; }
        
        /// <summary>
        /// Category of the fraud indicator
        /// </summary>
        public FraudIndicatorCategory Category { get; set; }
        
        /// <summary>
        /// Specific evidence supporting this indicator
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
        
        /// <summary>
        /// Elements affected by this fraud indicator
        /// </summary>
        public List<string> AffectedElements { get; set; } = new List<string>();
        
        /// <summary>
        /// Confidence level in this indicator (0-1 scale)
        /// </summary>
        public double Confidence { get; set; }
    }
}
