namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Assessment of a specific rule
    /// </summary>
    public class RuleAssessment
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the rule
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the rule is satisfied
        /// </summary>
        public bool IsSatisfied { get; set; }
        
        /// <summary>
        /// Evidence supporting the assessment
        /// </summary>
        public string Evidence { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed reasoning for the decision
        /// </summary>
        public string Reasoning { get; set; } = string.Empty;
        
        /// <summary>
        /// Weight assigned to this rule (importance)
        /// </summary>
        public int Weight { get; set; }
        
        /// <summary>
        /// Score for this rule (0-100)
        /// </summary>
        public int Score { get; set; }
    }
}
