using System.Text.Json.Serialization;
using BouwdepotInvoiceValidator.Domain.Attributes;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Response from document type verification
    /// </summary>
    public class DocumentTypeVerificationResponse
    {
        /// <summary>
        /// Type of document detected
        /// </summary>
        public string documentType { get; set; }

        /// <summary>
        /// Whether the document is an invoice
        /// </summary>
        public bool isInvoice { get; set; }

        /// <summary>
        /// Whether the document is a receipt
        /// </summary>
        public bool isReceipt { get; set; }
        
        /// <summary>
        /// Whether the document is a quotation/estimate
        /// </summary>
        public bool isQuotation { get; set; }

        /// <summary>
        /// Confidence score (0-100)
        /// </summary>
        public int confidence { get; set; }

        /// <summary>
        /// Explanation of the classification
        /// </summary>
        public string explanation { get; set; }
    }

    /// <summary>
    /// Response from invoice header extraction
    /// </summary>
    public class InvoiceHeaderResponse
    {
        /// <summary>
        /// Invoice number
        /// </summary>
        public string invoiceNumber { get; set; }

        /// <summary>
        /// Invoice date (YYYY-MM-DD)
        /// </summary>
        public string invoiceDate { get; set; }

        /// <summary>
        /// Due date (YYYY-MM-DD)
        /// </summary>
        public string dueDate { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal totalAmount { get; set; }

        /// <summary>
        /// Tax/VAT amount
        /// </summary>
        public decimal taxAmount { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        public string currency { get; set; }
    }

    /// <summary>
    /// Response from invoice parties extraction
    /// </summary>
    public class InvoicePartiesResponse
    {
        /// <summary>
        /// Vendor name
        /// </summary>
        public string vendorName { get; set; }

        /// <summary>
        /// Vendor address
        /// </summary>
        public string vendorAddress { get; set; }

        /// <summary>
        /// Vendor contact information
        /// </summary>
        public string vendorContact { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string customerName { get; set; }

        /// <summary>
        /// Customer address
        /// </summary>
        public string customerAddress { get; set; }
    }

    /// <summary>
    /// Response from invoice line items extraction
    /// </summary>
    [PromptSchema("Response from invoice line items extraction")]
    public class InvoiceLineItemsResponse
    {
        /// <summary>
        /// Line items
        /// </summary>
        [PromptProperty("Line items in the invoice", true)]
        public List<LineItemResponse> lineItems { get; set; }

        /// <summary>
        /// Payment terms
        /// </summary>
        [PromptProperty("Payment terms (e.g., Net 30 days)", false)]
        public string paymentTerms { get; set; }

        /// <summary>
        /// Payment method
        /// </summary>
        [PromptProperty("Payment method (e.g., Bank transfer, Credit card)", false)]
        public string paymentMethod { get; set; }

        /// <summary>
        /// Payment reference
        /// </summary>
        [PromptProperty("Payment reference number", false)]
        public string paymentReference { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [PromptProperty("Additional notes or important information", false)]
        public string notes { get; set; }

        /// <summary>
        /// Confidence score (0.0-1.0)
        /// </summary>
        [PromptProperty("Confidence score (0.0-1.0)", true)]
        public double confidence { get; set; }
    }

    /// <summary>
    /// Line item in invoice line items response
    /// </summary>
    [PromptSchema("Line item in an invoice")]
    public class LineItemResponse
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        [PromptProperty("Description of the item", true)]
        public string description { get; set; }

        /// <summary>
        /// Quantity of the item
        /// </summary>
        [PromptProperty("Quantity of the item", true)]
        public int quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        [PromptProperty("Price per unit", true)]
        public decimal unitPrice { get; set; }

        /// <summary>
        /// Total price
        /// </summary>
        [PromptProperty("Total price for this line item", true)]
        public decimal totalPrice { get; set; }
        
        /// <summary>
        /// VAT/tax rate applied to this item
        /// </summary>
        [PromptProperty("VAT/tax rate applied to this item", true)]
        public decimal vatRate { get; set; }
    }

    /// <summary>
    /// Response from fraud detection
    /// </summary>
    public class FraudDetectionResponse
    {
        /// <summary>
        /// Whether fraud is detected
        /// </summary>
        public bool possibleFraud { get; set; }

        /// <summary>
        /// Confidence score (0.0-1.0)
        /// </summary>
        public double confidence { get; set; }

        /// <summary>
        /// Description of visual evidence
        /// </summary>
        public string visualEvidence { get; set; }

        /// <summary>
        /// List of visual fraud indicators
        /// </summary>
        public List<string> visualIndicators { get; set; }
    }

    /// <summary>
    /// Response from line item analysis
    /// </summary>
    public class LineItemAnalysisResponse
    {
        /// <summary>
        /// Whether the line items are related to home improvement
        /// </summary>
        public bool isHomeImprovement { get; set; }

        /// <summary>
        /// Confidence score (0.0-1.0)
        /// </summary>
        public double confidence { get; set; }

        /// <summary>
        /// Summary of the analysis
        /// </summary>
        public string summary { get; set; }

        /// <summary>
        /// Primary purpose of the items
        /// </summary>
        public string primaryPurpose { get; set; }

        /// <summary>
        /// Categories represented in the line items
        /// </summary>
        public List<string> categories { get; set; }

        /// <summary>
        /// Analysis of each line item
        /// </summary>
        public List<LineItemAnalysis> lineItemAnalysis { get; set; }
    }

    /// <summary>
    /// Analysis of a single line item
    /// </summary>
    public class LineItemAnalysis
    {
        /// <summary>
        /// Original line item description
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Interpretation of the item
        /// </summary>
        public string interpretedAs { get; set; }

        /// <summary>
        /// Home improvement category
        /// </summary>
        public string category { get; set; }

        /// <summary>
        /// Whether the item is related to home improvement
        /// </summary>
        public bool isHomeImprovement { get; set; }

        /// <summary>
        /// Confidence score (0.0-1.0)
        /// </summary>
        public double confidence { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        public string notes { get; set; }
    }

    /// <summary>
    /// Response from home improvement analysis
    /// </summary>
    public class HomeImprovementResponse
    {
        /// <summary>
        /// Whether the invoice is related to home improvement
        /// </summary>
        public bool isHomeImprovement { get; set; }

        /// <summary>
        /// Categories eligible for Bouwdepot financing
        /// </summary>
        public List<string> eligibleCategories { get; set; }

        /// <summary>
        /// Assessment of each line item
        /// </summary>
        public List<LineItemAssessment> lineItemAssessment { get; set; }

        /// <summary>
        /// Total eligible amount
        /// </summary>
        public decimal totalEligibleAmount { get; set; }

        /// <summary>
        /// Total ineligible amount
        /// </summary>
        public decimal totalIneligibleAmount { get; set; }

        /// <summary>
        /// Overall confidence score (0.0-1.0)
        /// </summary>
        public double overallConfidence { get; set; }
    }

    /// <summary>
    /// Assessment of a single line item for Bouwdepot eligibility
    /// </summary>
    public class LineItemAssessment
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Whether the item is eligible for Bouwdepot financing
        /// </summary>
        public bool isEligible { get; set; }

        /// <summary>
        /// Home improvement category
        /// </summary>
        public string category { get; set; }

        /// <summary>
        /// Confidence score (0.0-1.0)
        /// </summary>
        public double confidence { get; set; }

        /// <summary>
        /// Reason for eligibility decision
        /// </summary>
        public string reason { get; set; }
    }

    /// <summary>
    /// Prompt template
    /// </summary>
    public class PromptTemplate
    {
        /// <summary>
        /// Metadata about the prompt
        /// </summary>
        public PromptMetadata metadata { get; set; }

        /// <summary>
        /// Template content
        /// </summary>
        public PromptContent template { get; set; }

        /// <summary>
        /// Example inputs and outputs
        /// </summary>
        public List<PromptExample> examples { get; set; }
    }

    /// <summary>
    /// Metadata about a prompt
    /// </summary>
    public class PromptMetadata
    {
        /// <summary>
        /// Name of the prompt
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Version of the prompt
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Description of the prompt
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Last modified date
        /// </summary>
        public string lastModified { get; set; }

        /// <summary>
        /// Author of the prompt
        /// </summary>
        public string author { get; set; }
    }

    /// <summary>
    /// Content of a prompt template
    /// </summary>
    public class PromptContent
    {
        /// <summary>
        /// Role of the AI
        /// </summary>
        public string role { get; set; }

        /// <summary>
        /// Task to perform
        /// </summary>
        public string task { get; set; }

        /// <summary>
        /// Instructions for the task
        /// </summary>
        public List<string> instructions { get; set; }

        // outputFormat property removed as it's now handled by the DynamicPromptService
    }

    /// <summary>
    /// Example input and output for a prompt
    /// </summary>
    public class PromptExample
    {
        /// <summary>
        /// Example input
        /// </summary>
        public string input { get; set; }

        /// <summary>
        /// Example output
        /// </summary>
        public object output { get; set; }
    }
}
