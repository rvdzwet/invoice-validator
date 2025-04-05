namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Configuration for validation thresholds
    /// </summary>
    public class ValidationThresholdConfiguration
    {
        /// <summary>
        /// Threshold for automatic approval (0-100)
        /// </summary>
        public int AutoApprovalThreshold { get; set; } = 75;
        
        /// <summary>
        /// Threshold for high risk (0-100)
        /// </summary>
        public int HighRiskThreshold { get; set; } = 40;
        
        /// <summary>
        /// Whether auto-approval is enabled
        /// </summary>
        public bool EnableAutoApproval { get; set; } = true;
        
        /// <summary>
        /// When the configuration was last modified
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
