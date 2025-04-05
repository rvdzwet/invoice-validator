namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Detailed content summary of what was purchased and why
    /// </summary>
    public class ContentSummary
    {
        /// <summary>
        /// Detailed description of what was purchased
        /// </summary>
        public string PurchasedItems { get; set; } = string.Empty;
        
        /// <summary>
        /// Analysis of the likely purpose/project
        /// </summary>
        public string IntendedPurpose { get; set; } = string.Empty;
        
        /// <summary>
        /// How these items impact the property value
        /// </summary>
        public string PropertyImpact { get; set; } = string.Empty;
        
        /// <summary>
        /// Category of home improvement (e.g., Kitchen Renovation, Bathroom Remodel)
        /// </summary>
        public string ProjectCategory { get; set; } = string.Empty;
        
        /// <summary>
        /// Estimated scope of the project (Small repair, Major renovation, etc.)
        /// </summary>
        public string EstimatedProjectScope { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this is likely part of a larger project
        /// </summary>
        public bool LikelyPartOfLargerProject { get; set; }
        
        /// <summary>
        /// Project stage (planning, in progress, finishing)
        /// </summary>
        public string ProjectStage { get; set; } = string.Empty;
    }
}
