namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Insights about the vendor from profiling
    /// </summary>
    public class VendorInsights
    {
        /// <summary>
        /// Name of the vendor
        /// </summary>
        public string VendorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Business categories the vendor operates in
        /// </summary>
        public List<string> BusinessCategories { get; set; } = new List<string>();
        
        /// <summary>
        /// Number of invoices previously processed from this vendor
        /// </summary>
        public int InvoiceCount { get; set; }
        
        /// <summary>
        /// When the vendor was first seen
        /// </summary>
        public DateTime FirstSeen { get; set; }
        
        /// <summary>
        /// When the vendor was last seen
        /// </summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>
        /// Reliability score for the vendor (0-1)
        /// </summary>
        public double ReliabilityScore { get; set; }
        
        /// <summary>
        /// Price stability score (0-1)
        /// </summary>
        public double PriceStabilityScore { get; set; }
        
        /// <summary>
        /// Quality of document score (0-1)
        /// </summary>
        public double DocumentQualityScore { get; set; }
        
        /// <summary>
        /// Whether unusual services were detected
        /// </summary>
        public bool UnusualServicesDetected { get; set; }
        
        /// <summary>
        /// Whether unreasonable prices were detected
        /// </summary>
        public bool UnreasonablePricesDetected { get; set; }
        
        /// <summary>
        /// Total number of anomalies detected for this vendor
        /// </summary>
        public int TotalAnomalyCount { get; set; }
        
        /// <summary>
        /// Services this vendor specializes in
        /// </summary>
        public List<string> VendorSpecialties { get; set; } = new List<string>();
    }
}
