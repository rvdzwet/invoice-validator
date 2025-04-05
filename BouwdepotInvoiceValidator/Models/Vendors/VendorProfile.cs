using System.Text.RegularExpressions;

namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Represents a vendor's profile with historical data
    /// </summary>
    public class VendorProfile
    {
        /// <summary>
        /// Unique identifier for the vendor profile
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        // Core vendor identification
        /// <summary>
        /// Name of the vendor
        /// </summary>
        public string VendorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Normalized name for fuzzy matching
        /// </summary>
        public string NormalizedName { get; set; } = string.Empty;
        
        /// <summary>
        /// Chamber of Commerce (KvK) number
        /// </summary>
        public string KvkNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// VAT number
        /// </summary>
        public string VatNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Known addresses for this vendor
        /// </summary>
        public HashSet<string> KnownAddresses { get; set; } = new();
        
        // Business category information
        /// <summary>
        /// Business categories the vendor operates in
        /// </summary>
        public List<string> BusinessCategories { get; set; } = new();
        
        /// <summary>
        /// Services this vendor specializes in
        /// </summary>
        public List<string> SpecialtyServices { get; set; } = new();
        
        // Service pattern analysis
        /// <summary>
        /// Common services provided by this vendor
        /// </summary>
        public Dictionary<string, ServicePattern> CommonServices { get; set; } = new();
        
        // Price pattern analysis
        /// <summary>
        /// Price ranges for different services/items
        /// </summary>
        public Dictionary<string, PriceRange> PriceRanges { get; set; } = new();
        
        // Known payment methods
        /// <summary>
        /// Payment details previously used by this vendor
        /// </summary>
        public List<PaymentDetails> PaymentDetails { get; set; } = new();
        
        // Invoice pattern recognition
        /// <summary>
        /// Recurring line item patterns
        /// </summary>
        public List<string> RecurringLineItemPatterns { get; set; } = new();
        
        /// <summary>
        /// Invoice formatting characteristics
        /// </summary>
        public InvoiceFormatting InvoiceFormatting { get; set; } = new();
        
        // Trust metrics
        /// <summary>
        /// Trust metrics for this vendor
        /// </summary>
        public VendorTrustMetrics TrustMetrics { get; set; } = new();
        
        // Historical data
        /// <summary>
        /// Number of invoices processed
        /// </summary>
        public int InvoiceCount { get; set; }
        
        /// <summary>
        /// When this vendor was first seen
        /// </summary>
        public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When this vendor was last seen
        /// </summary>
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When this profile was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Creates a normalized vendor name for matching purposes
        /// </summary>
        /// <param name="name">The original vendor name</param>
        /// <returns>Normalized vendor name</returns>
        public static string NormalizeVendorName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
                
            // Remove legal entity indicators
            var normalized = Regex.Replace(name, @"\b(BV|B\.V\.|V\.O\.F\.|VOF|N\.V\.|NV|B\.V|Inc\.|LLC|Ltd\.)\b", "", RegexOptions.IgnoreCase);
            
            // Remove special characters, keep only letters, numbers and spaces
            normalized = Regex.Replace(normalized, @"[^\w\s]", "");
            
            // Consolidate spaces and trim
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim().ToLowerInvariant();
            
            return normalized;
        }
    }

    /// <summary>
    /// Represents a service pattern with frequency information
    /// </summary>
    public class ServicePattern
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;
        
        /// <summary>
        /// How frequently this service appears in invoices
        /// </summary>
        public int Frequency { get; set; }
        
        /// <summary>
        /// When this service was first seen
        /// </summary>
        public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When this service was last seen
        /// </summary>
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Related keywords for this service
        /// </summary>
        public List<string> RelatedKeywords { get; set; } = new();
    }

    /// <summary>
    /// Represents a price range for a category of items
    /// </summary>
    public class PriceRange
    {
        /// <summary>
        /// Category of the item
        /// </summary>
        public string ItemCategory { get; set; } = string.Empty;
        
        /// <summary>
        /// Minimum price observed
        /// </summary>
        public decimal MinPrice { get; set; }
        
        /// <summary>
        /// Maximum price observed
        /// </summary>
        public decimal MaxPrice { get; set; }
        
        /// <summary>
        /// Average price
        /// </summary>
        public decimal AveragePrice { get; set; }
        
        /// <summary>
        /// Standard deviation of prices
        /// </summary>
        public decimal StandardDeviation { get; set; }
        
        /// <summary>
        /// Number of samples used to calculate statistics
        /// </summary>
        public int SampleSize { get; set; }
    }

    /// <summary>
    /// Represents invoice formatting characteristics
    /// </summary>
    public class InvoiceFormatting
    {
        /// <summary>
        /// Common elements in the header
        /// </summary>
        public List<string> CommonHeaderElements { get; set; } = new();
        
        /// <summary>
        /// Common elements in the footer
        /// </summary>
        public List<string> CommonFooterElements { get; set; } = new();
        
        /// <summary>
        /// Whether invoices typically include logos
        /// </summary>
        public bool UsesLogos { get; set; }
        
        /// <summary>
        /// Whether invoices typically include digital signatures
        /// </summary>
        public bool UsesDigitalSignatures { get; set; }
        
        /// <summary>
        /// Typical layout description
        /// </summary>
        public string TypicalLayout { get; set; } = string.Empty;
        
        /// <summary>
        /// Identified fonts used in invoices
        /// </summary>
        public List<string> IdentifiedFonts { get; set; } = new();
    }

    /// <summary>
    /// Trust metrics for a vendor
    /// </summary>
    public class VendorTrustMetrics
    {
        /// <summary>
        /// Overall reliability score (0-1)
        /// </summary>
        public double ReliabilityScore { get; set; } = 0.5; // Default middle value
        
        /// <summary>
        /// Consistency of service score (0-1)
        /// </summary>
        public double ConsistencyScore { get; set; } = 0.5;
        
        /// <summary>
        /// Price stability score (0-1)
        /// </summary>
        public double PriceStabilityScore { get; set; } = 0.5;
        
        /// <summary>
        /// Quality of documentation score (0-1)
        /// </summary>
        public double DocumentQualityScore { get; set; } = 0.5;
        
        /// <summary>
        /// Expertise scores by category
        /// </summary>
        public Dictionary<string, double> CategoryExpertiseScores { get; set; } = new();
        
        /// <summary>
        /// Detected anomalies for this vendor
        /// </summary>
        public List<AnomalyRecord> DetectedAnomalies { get; set; } = new();
    }

    /// <summary>
    /// Record of an anomaly detected for a vendor
    /// </summary>
    public class AnomalyRecord
    {
        /// <summary>
        /// Type of anomaly
        /// </summary>
        public string AnomalyType { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the anomaly
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// When the anomaly was detected
        /// </summary>
        public DateTime DetectedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Severity of the anomaly (0-1)
        /// </summary>
        public double Severity { get; set; }
        
        /// <summary>
        /// Whether the anomaly has been resolved
        /// </summary>
        public bool Resolved { get; set; }
    }
}
