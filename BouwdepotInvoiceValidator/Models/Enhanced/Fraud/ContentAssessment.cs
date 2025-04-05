namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Assessment of invoice content
    /// </summary>
    public class ContentAssessment
    {
        /// <summary>
        /// Whether missing elements were detected
        /// </summary>
        public bool HasMissingElements { get; set; }
        
        /// <summary>
        /// Whether inconsistent dates were detected
        /// </summary>
        public bool HasInconsistentDates { get; set; }
        
        /// <summary>
        /// Whether mathematical errors were detected
        /// </summary>
        public bool HasMathematicalErrors { get; set; }
        
        /// <summary>
        /// Whether suspicious line items were detected
        /// </summary>
        public bool HasSuspiciousLineItems { get; set; }
        
        /// <summary>
        /// Whether vague descriptions were detected
        /// </summary>
        public bool HasVagueDescriptions { get; set; }
        
        /// <summary>
        /// Whether ambiguous services were detected
        /// </summary>
        public bool HasAmbiguousServices { get; set; }
        
        /// <summary>
        /// List of content issues
        /// </summary>
        public List<string> ContentIssues { get; set; } = new List<string>();
    }
}
