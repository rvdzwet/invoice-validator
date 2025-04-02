using System.Collections.Generic;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Vendors
{
    /// <summary>
    /// Interface for vendor profiling operations
    /// </summary>
    public interface IVendorProfileService
    {
        /// <summary>
        /// Gets a vendor profile based on an invoice
        /// </summary>
        /// <param name="invoice">The invoice containing vendor information</param>
        /// <returns>The vendor profile (creates new one if not found)</returns>
        Task<VendorProfile> GetVendorProfileAsync(Invoice invoice);
        
        /// <summary>
        /// Updates a vendor profile with new invoice data
        /// </summary>
        /// <param name="invoice">The invoice with the vendor data</param>
        /// <param name="validationResult">The validation result for analysis</param>
        /// <returns>The updated vendor profile</returns>
        Task<VendorProfile> UpdateVendorProfileAsync(Invoice invoice, ValidationResult validationResult);
        
        /// <summary>
        /// Analyzes a vendor's trustworthiness using historical data
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <returns>The vendor trust analysis result</returns>
        Task<VendorTrustAnalysisResult> AnalyzeVendorTrustworthinessAsync(Invoice invoice);
        
        /// <summary>
        /// Analyzes whether prices in the invoice are reasonable based on vendor history
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <returns>The price analysis result</returns>
        Task<PriceAnalysisResult> AnalyzePriceReasonablenessAsync(Invoice invoice);
        
        /// <summary>
        /// Analyzes whether services in the invoice match the vendor's usual patterns
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <returns>The service pattern analysis result</returns>
        Task<ServiceAnalysisResult> AnalyzeServicePatternsAsync(Invoice invoice);
        
        /// <summary>
        /// Detects anomalies in the invoice compared to the vendor's profile
        /// </summary>
        /// <param name="invoice">The invoice to analyze</param>
        /// <param name="profile">The vendor profile</param>
        /// <returns>List of detected anomalies</returns>
        Task<List<AnomalyRecord>> DetectVendorAnomaliesAsync(Invoice invoice, VendorProfile profile);
        
        /// <summary>
        /// Gets insights about a vendor for display/reporting
        /// </summary>
        /// <param name="vendorId">The vendor identifier</param>
        /// <returns>Vendor insights summary</returns>
        Task<VendorInsights> GetVendorInsightsAsync(string vendorId);
        
        /// <summary>
        /// Gets a count of known vendors in the system
        /// </summary>
        /// <returns>The number of vendor profiles</returns>
        Task<int> GetVendorCountAsync();
    }
    
    /// <summary>
    /// Result of vendor trust analysis
    /// </summary>
    public class VendorTrustAnalysisResult
    {
        /// <summary>
        /// Overall trust score for the vendor (0-1)
        /// </summary>
        public double OverallTrustScore { get; set; }
        
        /// <summary>
        /// Trust scores for individual aspects
        /// </summary>
        public Dictionary<string, double> TrustScores { get; set; } = new Dictionary<string, double>();
        
        /// <summary>
        /// Factors supporting trust
        /// </summary>
        public List<string> TrustFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Factors raising concerns
        /// </summary>
        public List<string> ConcernFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Number of invoices analyzed to generate this result
        /// </summary>
        public int InvoiceCount { get; set; }
    }
    
    /// <summary>
    /// Result of price reasonableness analysis
    /// </summary>
    public class PriceAnalysisResult
    {
        /// <summary>
        /// Whether unreasonable prices were detected
        /// </summary>
        public bool UnreasonablePricesDetected { get; set; }
        
        /// <summary>
        /// Line items with unreasonable prices
        /// </summary>
        public List<UnreasonablyPricedItem> UnreasonablyPricedItems { get; set; } = new List<UnreasonablyPricedItem>();
        
        /// <summary>
        /// Average price deviation percentage across all line items
        /// </summary>
        public double AveragePriceDeviation { get; set; }
        
        /// <summary>
        /// Maximum price deviation percentage among all line items
        /// </summary>
        public double MaxPriceDeviation { get; set; }
    }
    
    /// <summary>
    /// Line item with an unreasonable price
    /// </summary>
    public class UnreasonablyPricedItem
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The actual price charged
        /// </summary>
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// The expected minimum price based on history
        /// </summary>
        public decimal ExpectedMinPrice { get; set; }
        
        /// <summary>
        /// The expected maximum price based on history
        /// </summary>
        public decimal ExpectedMaxPrice { get; set; }
        
        /// <summary>
        /// The deviation percentage from the average price
        /// </summary>
        public double DeviationPercentage { get; set; }
    }
    
    /// <summary>
    /// Result of service pattern analysis
    /// </summary>
    public class ServiceAnalysisResult
    {
        /// <summary>
        /// Whether unusual services were detected
        /// </summary>
        public bool UnusualServicesDetected { get; set; }
        
        /// <summary>
        /// List of unusual services (not typical for this vendor)
        /// </summary>
        public List<string> UnusualServices { get; set; } = new List<string>();
        
        /// <summary>
        /// Services this vendor commonly provides
        /// </summary>
        public List<string> CommonServices { get; set; } = new List<string>();
        
        /// <summary>
        /// Line items that match this vendor's common services
        /// </summary>
        public List<string> MatchingServices { get; set; } = new List<string>();
        
        /// <summary>
        /// Overall service pattern score (0-1)
        /// </summary>
        public double ServicePatternScore { get; set; }
    }
}
