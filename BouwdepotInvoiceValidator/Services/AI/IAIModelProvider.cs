using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Interface for AI model providers that can analyze invoices
    /// This abstraction allows switching between different AI models (Gemini, OpenAI, etc.)
    /// </summary>
    public interface IAIModelProvider
    {
        /// <summary>
        /// Get provider name and model information
        /// </summary>
        /// <returns>Provider information object</returns>
        ModelProviderInfo GetProviderInfo();
        
        /// <summary>
        /// Validates an invoice for home improvement eligibility using unified approach
        /// </summary>
        /// <param name="invoice">The invoice to validate</param>
        /// <param name="options">Optional validation options</param>
        /// <returns>Comprehensive validation result</returns>
        Task<InvoiceValidationResponse> ValidateInvoiceAsync(Invoice invoice, ValidationOptions options = null);
        
        /// <summary>
        /// Extracts data from an invoice
        /// </summary>
        /// <param name="invoice">The invoice with raw data/images</param>
        /// <returns>Invoice with extracted data</returns>
        Task<Invoice> ExtractDataFromInvoiceAsync(Invoice invoice);
    }
    
    /// <summary>
    /// Options for validation process
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        /// Language code (e.g., "nl-NL", "en-US")
        /// </summary>
        public string LanguageCode { get; set; } = "nl-NL";
        
        /// <summary>
        /// Minimum confidence threshold (0-100)
        /// </summary>
        public int ConfidenceThreshold { get; set; } = 70;
        
        /// <summary>
        /// Whether to perform visual analysis on images
        /// </summary>
        public bool IncludeVisualAnalysis { get; set; } = true;
        
        /// <summary>
        /// Whether to check for fraud indicators
        /// </summary>
        public bool DetectFraud { get; set; } = true;
        
        /// <summary>
        /// Whether to perform detailed line item analysis
        /// </summary>
        public bool AnalyzeLineItems { get; set; } = true;
        
        /// <summary>
        /// Maximum number of line items to analyze
        /// </summary>
        public int MaxLineItems { get; set; } = 30;
        
        /// <summary>
        /// Whether to include audit-ready justifications
        /// </summary>
        public bool IncludeAuditJustification { get; set; } = true;
        
        /// <summary>
        /// Additional context or instructions for the model
        /// </summary>
        public string AdditionalContext { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Information about the model provider
    /// </summary>
    public class ModelProviderInfo
    {
        /// <summary>
        /// Name of the provider (e.g., "Gemini", "OpenAI")
        /// </summary>
        public string ProviderName { get; set; }
        
        /// <summary>
        /// Model identifier
        /// </summary>
        public string ModelId { get; set; }
        
        /// <summary>
        /// Model version
        /// </summary>
        public string ModelVersion { get; set; }
        
        /// <summary>
        /// Capabilities of the model
        /// </summary>
        public List<string> Capabilities { get; set; } = new List<string>();
        
        /// <summary>
        /// Model context limitation
        /// </summary>
        public int MaxContextLength { get; set; }
        
        /// <summary>
        /// Whether the model supports image analysis
        /// </summary>
        public bool SupportsImages { get; set; }
    }
    
    /// <summary>
    /// Response from the invoice validation process
    /// </summary>
    public class InvoiceValidationResponse
    {
        /// <summary>
        /// Whether this is a valid invoice document
        /// </summary>
        public bool IsValidInvoice { get; set; }
        
        /// <summary>
        /// Whether the invoice is for home improvement expenses
        /// </summary>
        public bool IsHomeImprovement { get; set; }
        
        /// <summary>
        /// Overall confidence score (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Extracted data from the invoice
        /// </summary>
        public Invoice ExtractedInvoice { get; set; }
        
        /// <summary>
        /// Analysis of line items
        /// </summary>
        public List<LineItemValidation> LineItemAnalysis { get; set; } = new List<LineItemValidation>();
        
        /// <summary>
        /// Categories of items found in the invoice
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();
        
        /// <summary>
        /// Primary purpose of the invoice
        /// </summary>
        public string PrimaryPurpose { get; set; }
        
        /// <summary>
        /// Summary of the validation
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Detailed justification for audit purposes
        /// </summary>
        public string AuditJustification { get; set; }
        
        /// <summary>
        /// Notes from the validation process
        /// </summary>
        public List<ValidationNote> ValidationNotes { get; set; } = new List<ValidationNote>();
        
        /// <summary>
        /// Detected fraud indicators
        /// </summary>
        public List<FraudIndicator> FraudIndicators { get; set; } = new List<FraudIndicator>();
        
        /// <summary>
        /// Criteria assessments showing how different aspects were evaluated
        /// </summary>
        public List<CriterionAssessment> CriteriaAssessments { get; set; } = new List<CriterionAssessment>();
        
        /// <summary>
        /// Bouwdepot-specific validation details
        /// </summary>
        public BouwdepotValidationDetails BouwdepotValidation { get; set; }
        
        /// <summary>
        /// Raw response from the AI model
        /// </summary>
        public string RawResponse { get; set; }
    }
    
    /// <summary>
    /// Analysis result for a single line item
    /// </summary>
    public class LineItemValidation
    {
        /// <summary>
        /// Original description from the invoice
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Interpreted type of item
        /// </summary>
        public string InterpretedAs { get; set; }
        
        /// <summary>
        /// Category of the item
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Whether the item is eligible for home improvement
        /// </summary>
        public bool IsEligible { get; set; }
        
        /// <summary>
        /// Confidence score (0-100)
        /// </summary>
        public int Confidence { get; set; }
        
        /// <summary>
        /// Analysis notes
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Whether the item is permanently attached
        /// </summary>
        public bool IsPermanentlyAttached { get; set; }
        
        /// <summary>
        /// Whether the item improves home quality
        /// </summary>
        public bool ImproveHomeQuality { get; set; }
    }
    
    /// <summary>
    /// Validation note with severity level
    /// </summary>
    public class ValidationNote
    {
        /// <summary>
        /// Severity of the note
        /// </summary>
        public ValidationSeverity Severity { get; set; }
        
        /// <summary>
        /// Message content
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Related section or property
        /// </summary>
        public string Section { get; set; }
    }
    
    /// <summary>
    /// Indicator of potential fraud
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Name of the indicator
        /// </summary>
        public string IndicatorName { get; set; }
        
        /// <summary>
        /// Description of the fraud indicator
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Evidence supporting the indicator
        /// </summary>
        public string Evidence { get; set; }
        
        /// <summary>
        /// Severity level (0-1.0)
        /// </summary>
        public double Severity { get; set; }
        
        /// <summary>
        /// Category of fraud indicator
        /// </summary>
        public FraudIndicatorCategory Category { get; set; }
    }
    
    /// <summary>
    /// Category of fraud indicator
    /// </summary>
    public enum FraudIndicatorCategory
    {
        DocumentIssue,
        ContentIssue,
        PricingIssue,
        VendorIssue,
        VisualIssue,
        Other
    }
}
