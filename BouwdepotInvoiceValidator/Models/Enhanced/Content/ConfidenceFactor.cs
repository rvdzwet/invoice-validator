namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// A factor that contributed to the confidence score
    /// </summary>
    public class ConfidenceFactor
    {
        /// <summary>
        /// Name of the factor
        /// </summary>
        public string FactorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Impact on the confidence score (-20 to +20)
        /// </summary>
        public int Impact { get; set; }
        
        /// <summary>
        /// Explanation of why this factor was considered
        /// </summary>
        public string Explanation { get; set; } = string.Empty;
    }
}
