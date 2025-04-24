using System.Text.Json.Serialization;
using BouwdepotInvoiceValidator.Domain.Attributes;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Comprehensive response model for withdrawal proof validation
    /// </summary>
    [PromptSchema("Comprehensive Withdrawal Proof Validation")]
    public class ComprehensiveWithdrawalProofResponse
    {
        /// <summary>
        /// Document analysis details
        /// </summary>
        [PromptProperty("Document analysis details", true)]
        [JsonPropertyName("documentAnalysis")]
        public DocumentAnalysis DocumentAnalysis { get; set; }
        
        /// <summary>
        /// Construction activities analysis
        /// </summary>
        [PromptProperty("Construction activities analysis", true)]
        [JsonPropertyName("constructionActivities")]
        public ConstructionActivities ConstructionActivities { get; set; }
        
        /// <summary>
        /// Fraud analysis details
        /// </summary>
        [PromptProperty("Fraud analysis details", true)]
        [JsonPropertyName("fraudAnalysis")]
        public FraudAnalysis FraudAnalysis { get; set; }
        
        /// <summary>
        /// Eligibility determination
        /// </summary>
        [PromptProperty("Eligibility determination", true)]
        [JsonPropertyName("eligibilityDetermination")]
        public EligibilityDetermination EligibilityDetermination { get; set; }
        
        /// <summary>
        /// Audit summary
        /// </summary>
        [PromptProperty("Audit summary", true)]
        [JsonPropertyName("auditSummary")]
        public AuditSummary AuditSummary { get; set; }
    }

    /// <summary>
    /// Document analysis section
    /// </summary>
    [PromptSchema("Document analysis section with extracted metadata and content recognition")]
    public class DocumentAnalysis
    {
        /// <summary>
        /// Type of the primary document found (invoice, receipt, quotation, other, or null if multiple/none detected)
        /// </summary>
        [JsonPropertyName("documentType")]
        [PromptProperty("Type of the primary document found (invoice, receipt, quotation, other, or null if multiple/none detected)")]
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Confidence in primary document type classification (0-100), or null
        /// </summary>
        [JsonPropertyName("confidence")]
        [PromptProperty("Confidence in primary document type classification (0-100), or null")]
        public int? Confidence { get; set; }
        
        /// <summary>
        /// Primary language (e.g., Dutch, English, Auto), or null
        /// </summary>
        [JsonPropertyName("language")]
        [PromptProperty("Primary language (e.g., nl-NL, en-UK, de-DE), use ISO standards, or null")]
        public string Language { get; set; }
        
        /// <summary>
        /// Invoice/receipt/quote number, or null
        /// </summary>
        [JsonPropertyName("documentNumber")]
        [PromptProperty("Invoice/receipt/quote number, or null")]
        public string DocumentNumber { get; set; }
        
        /// <summary>
        /// Date issued (YYYY-MM-DD format), or null
        /// </summary>
        [JsonPropertyName("issueDate")]
        [PromptProperty("Date issued (YYYY-MM-DD format), or null")]
        public string IssueDate { get; set; }
        
        /// <summary>
        /// Due date (YYYY-MM-DD format), or null
        /// </summary>
        [JsonPropertyName("dueDate")]
        [PromptProperty("Due date (YYYY-MM-DD format), or null")]
        public string DueDate { get; set; }
        
        /// <summary>
        /// Total amount, including tax, or null
        /// </summary>
        [JsonPropertyName("totalAmount")]
        [PromptProperty("Total amount, including tax, or null")]
        public decimal? TotalAmount { get; set; }
        
        /// <summary>
        /// Currency code (e.g., EUR), or null
        /// </summary>
        [JsonPropertyName("currency")]
        [PromptProperty("Currency code (e.g., EUR), or null")]
        public string Currency { get; set; }
        
        /// <summary>
        /// Vendor details
        /// </summary>
        [JsonPropertyName("vendor")]
        [PromptProperty("Vendor details", true)]
        public VendorInfo Vendor { get; set; }
        
        /// <summary>
        /// Customer details
        /// </summary>
        [JsonPropertyName("customer")]
        [PromptProperty("Customer details", true)]
        public CustomerInfo Customer { get; set; }
        
        /// <summary>
        /// Array of extracted line items, or null
        /// </summary>
        [JsonPropertyName("lineItems")]
        [PromptProperty("Array of extracted line items, or null")]
        public List<DocumentLineItem> LineItems { get; set; }
        
        /// <summary>
        /// Subtotal or null
        /// </summary>
        [JsonPropertyName("subtotal")]
        [PromptProperty("Subtotal or null")]
        public decimal? Subtotal { get; set; }
        
        /// <summary>
        /// Total tax amount or null
        /// </summary>
        [JsonPropertyName("taxAmount")]
        [PromptProperty("Total tax amount or null")]
        public decimal? TaxAmount { get; set; }
        
        /// <summary>
        /// Payment terms or null
        /// </summary>
        [JsonPropertyName("paymentTerms")]
        [PromptProperty("Payment terms or null")]
        public string PaymentTerms { get; set; }
        
        /// <summary>
        /// Notes or null
        /// </summary>
        [JsonPropertyName("notes")]
        [PromptProperty("Notes or null")]
        public string Notes { get; set; }
        
        /// <summary>
        /// True if multiple docs found
        /// </summary>
        [JsonPropertyName("multipleDocumentsDetected")]
        [PromptProperty("True if multiple docs found", true)]
        public bool MultipleDocumentsDetected { get; set; }
        
        /// <summary>
        /// Count of distinct documents
        /// </summary>
        [JsonPropertyName("detectedDocumentCount")]
        [PromptProperty("Count of distinct documents", true)]
        public int DetectedDocumentCount { get; set; }
    }

    /// <summary>
    /// Vendor information
    /// </summary>
    [PromptSchema("Vendor information with identification details")]
    public class VendorInfo
    {
        /// <summary>
        /// Vendor name or placeholder
        /// </summary>
        [JsonPropertyName("name")]
        [PromptProperty("Vendor name or placeholder", true)]
        public string Name { get; set; }
        
        /// <summary>
        /// Vendor address or null
        /// </summary>
        [JsonPropertyName("address")]
        [PromptProperty("Vendor address or null")]
        public string Address { get; set; }
        
        /// <summary>
        /// KvK number or null
        /// </summary>
        [JsonPropertyName("kvkNumber")]
        [PromptProperty("KvK number or null")]
        public string KvkNumber { get; set; }
        
        /// <summary>
        /// BTW number or null
        /// </summary>
        [JsonPropertyName("btwNumber")]
        [PromptProperty("BTW number or null")]
        public string BtwNumber { get; set; }
        
        /// <summary>
        /// Vendor contact info or null
        /// </summary>
        [JsonPropertyName("contact")]
        [PromptProperty("Vendor contact info or null")]
        public string Contact { get; set; }
    }

    /// <summary>
    /// Customer information
    /// </summary>
    [PromptSchema("Customer information for the invoice recipient")]
    public class CustomerInfo
    {
        /// <summary>
        /// Customer name or placeholder
        /// </summary>
        [JsonPropertyName("name")]
        [PromptProperty("Customer name or placeholder", true)]
        public string Name { get; set; }
        
        /// <summary>
        /// Customer address or null
        /// </summary>
        [JsonPropertyName("address")]
        [PromptProperty("Customer address or null")]
        public string Address { get; set; }
    }

    /// <summary>
    /// Document line item
    /// </summary>
    [PromptSchema("Line item from invoice or receipt with pricing details")]
    public class DocumentLineItem
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        [JsonPropertyName("description")]
        [PromptProperty("Description of the item", true)]
        public string Description { get; set; }
        
        /// <summary>
        /// Quantity of the item
        /// </summary>
        [JsonPropertyName("quantity")]
        [PromptProperty("Quantity of the item", true)]
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Unit price of the item
        /// </summary>
        [JsonPropertyName("unitPrice")]
        [PromptProperty("Unit price of the item", true)]
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Tax rate applied to this item, or null
        /// </summary>
        [JsonPropertyName("taxRate")]
        [PromptProperty("Tax rate applied to this item, or null")]
        public decimal? TaxRate { get; set; }
        
        /// <summary>
        /// Total price for this line item
        /// </summary>
        [JsonPropertyName("totalPrice")]
        [PromptProperty("Total price for this line item", true)]
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// Line item tax amount, or null
        /// </summary>
        [JsonPropertyName("lineItemTaxAmount")]
        [PromptProperty("Line item tax amount, or null")]
        public decimal? LineItemTaxAmount { get; set; }
        
        /// <summary>
        /// Line item total with tax, or null
        /// </summary>
        [JsonPropertyName("lineItemTotalWithTax")]
        [PromptProperty("Line item total with tax, or null")]
        public decimal? LineItemTotalWithTax { get; set; }
    }

    /// <summary>
    /// Construction activities section
    /// </summary>
    [PromptSchema("Analysis of construction-related activities and eligibility")]
    public class ConstructionActivities
    {
        /// <summary>
        /// Whether any item is construction related
        /// </summary>
        [JsonPropertyName("isConstructionRelatedOverall")]
        [PromptProperty("Whether any item is construction related", true)]
        public bool IsConstructionRelatedOverall { get; set; }
        
        /// <summary>
        /// Sum of eligible amounts
        /// </summary>
        [JsonPropertyName("totalEligibleAmountCalculated")]
        [PromptProperty("Sum of eligible amounts", true)]
        public decimal TotalEligibleAmountCalculated { get; set; }
        
        /// <summary>
        /// Percentage eligible
        /// </summary>
        [JsonPropertyName("percentageEligibleCalculated")]
        [PromptProperty("Percentage eligible", true)]
        public decimal PercentageEligibleCalculated { get; set; }
        
        /// <summary>
        /// Analysis per line item, or empty array
        /// </summary>
        [JsonPropertyName("detailedActivityAnalysis")]
        [PromptProperty("Analysis per line item, or empty array", true)]
        public List<ActivityAnalysis> DetailedActivityAnalysis { get; set; }
    }

    /// <summary>
    /// Activity analysis for a line item
    /// </summary>
    [PromptSchema("Detailed analysis of an individual construction activity")]
    public class ActivityAnalysis
    {
        /// <summary>
        /// Original description of the item
        /// </summary>
        [JsonPropertyName("originalDescription")]
        [PromptProperty("Original description of the item", true)]
        public string OriginalDescription { get; set; }
        
        /// <summary>
        /// Categorization of the activity
        /// </summary>
        [JsonPropertyName("categorization")]
        [PromptProperty("Categorization of the activity", true)]
        public string Categorization { get; set; }
        
        /// <summary>
        /// Whether the item is eligible
        /// </summary>
        [JsonPropertyName("isEligible")]
        [PromptProperty("Whether the item is eligible", true)]
        public bool IsEligible { get; set; }
        
        /// <summary>
        /// Eligible amount for this item
        /// </summary>
        [JsonPropertyName("eligibleAmountForItem")]
        [PromptProperty("Eligible amount for this item", true)]
        public decimal EligibleAmountForItem { get; set; }
        
        /// <summary>
        /// Ineligible amount for this item
        /// </summary>
        [JsonPropertyName("ineligibleAmountForItem")]
        [PromptProperty("Ineligible amount for this item", true)]
        public decimal IneligibleAmountForItem { get; set; }
        
        /// <summary>
        /// Confidence in the eligibility determination
        /// </summary>
        [JsonPropertyName("confidence")]
        [PromptProperty("Confidence in the eligibility determination", true)]
        public decimal Confidence { get; set; }
        
        /// <summary>
        /// Reasoning for the eligibility determination
        /// </summary>
        [JsonPropertyName("reasoningForEligibility")]
        [PromptProperty("Reasoning for the eligibility determination", true)]
        public string ReasoningForEligibility { get; set; }
    }

    /// <summary>
    /// Fraud analysis section
    /// </summary>
    [PromptSchema("Analysis of potential fraud indicators")]
    public class FraudAnalysis
    {
        /// <summary>
        /// Overall fraud risk level or NotApplicable
        /// </summary>
        [JsonPropertyName("fraudRiskLevel")]
        [PromptProperty("Overall fraud risk level or NotApplicable", true)]
        public string FraudRiskLevel { get; set; }
        
        /// <summary>
        /// Fraud risk score or 0
        /// </summary>
        [JsonPropertyName("fraudRiskScore")]
        [PromptProperty("Fraud risk score or 0", true)]
        public decimal FraudRiskScore { get; set; }
        
        /// <summary>
        /// List of indicators, or empty array
        /// </summary>
        [JsonPropertyName("indicatorsFound")]
        [PromptProperty("List of indicators, or empty array", true)]
        public List<FraudIndicatorInfo> IndicatorsFound { get; set; }
        
        /// <summary>
        /// Summary of fraud analysis
        /// </summary>
        [JsonPropertyName("summary")]
        [PromptProperty("Summary of fraud analysis", true)]
        public string Summary { get; set; }
    }

    /// <summary>
    /// Fraud indicator information
    /// </summary>
    [PromptSchema("Information about a specific fraud indicator")]
    public class FraudIndicatorInfo
    {
        /// <summary>
        /// Category of the fraud indicator
        /// </summary>
        [JsonPropertyName("category")]
        [PromptProperty("Category of the fraud indicator", true)]
        public string Category { get; set; }
        
        /// <summary>
        /// Description of the fraud indicator
        /// </summary>
        [JsonPropertyName("description")]
        [PromptProperty("Description of the fraud indicator", true)]
        public string Description { get; set; }
        
        /// <summary>
        /// Confidence in the fraud indicator detection
        /// </summary>
        [JsonPropertyName("confidence")]
        [PromptProperty("Confidence in the fraud indicator detection", true)]
        public decimal Confidence { get; set; }
        
        /// <summary>
        /// Implication of the fraud indicator
        /// </summary>
        [JsonPropertyName("implication")]
        [PromptProperty("Implication of the fraud indicator", true)]
        public string Implication { get; set; }
    }

    /// <summary>
    /// Eligibility determination section
    /// </summary>
    [PromptSchema("Final determination of eligibility for construction fund withdrawal")]
    public class EligibilityDetermination
    {
        /// <summary>
        /// Final overall eligibility status (Eligible, Partially Eligible, Ineligible)
        /// </summary>
        [JsonPropertyName("overallStatus")]
        [PromptProperty("Final overall eligibility status (Eligible, Partially Eligible, Ineligible)", true)]
        public string OverallStatus { get; set; }
        
        /// <summary>
        /// Overall confidence score (0-100) in the final determination
        /// </summary>
        [JsonPropertyName("decisionConfidenceScore")]
        [PromptProperty("Overall confidence score (0-100) in the final determination", true)]
        public decimal DecisionConfidenceScore { get; set; }
        
        /// <summary>
        /// The final eligible amount (0 if ineligible)
        /// </summary>
        [JsonPropertyName("totalEligibleAmountDetermined")]
        [PromptProperty("The final eligible amount (0 if ineligible)", true)]
        public decimal TotalEligibleAmountDetermined { get; set; }
        
        /// <summary>
        /// The final ineligible amount
        /// </summary>
        [JsonPropertyName("totalIneligibleAmountDetermined")]
        [PromptProperty("The final ineligible amount", true)]
        public decimal TotalIneligibleAmountDetermined { get; set; }
        
        /// <summary>
        /// Total amount from the primary document (0 if multiple docs)
        /// </summary>
        [JsonPropertyName("totalDocumentAmountReviewed")]
        [PromptProperty("Total amount from the primary document (0 if multiple docs)", true)]
        public decimal TotalDocumentAmountReviewed { get; set; }
        
        /// <summary>
        /// Categorization of the primary reason for the eligibility determination
        /// </summary>
        [JsonPropertyName("rationaleCategory")]
        [PromptProperty("Categorization of the primary reason for the eligibility determination", true)]
        public string RationaleCategory { get; set; }
        
        /// <summary>
        /// A comprehensive summary explaining the eligibility decision
        /// </summary>
        [JsonPropertyName("rationaleSummary")]
        [PromptProperty("A comprehensive summary explaining the eligibility decision", true)]
        public string RationaleSummary { get; set; }
        
        /// <summary>
        /// Specific actions recommended
        /// </summary>
        [JsonPropertyName("requiredActions")]
        [PromptProperty("Specific actions recommended", true)]
        public List<string> RequiredActions { get; set; }
        
        /// <summary>
        /// Specific notes for an auditor
        /// </summary>
        [JsonPropertyName("notesForAuditor")]
        [PromptProperty("Specific notes for an auditor")]
        public string NotesForAuditor { get; set; }
    }

    /// <summary>
    /// Audit summary section
    /// </summary>
    [PromptSchema("Summary of validation findings for audit purposes")]
    public class AuditSummary
    {
        /// <summary>
        /// Summary of validation process and outcome
        /// </summary>
        [JsonPropertyName("overallValidationSummary")]
        [PromptProperty("Summary of validation process and outcome", true)]
        public string OverallValidationSummary { get; set; }
        
        /// <summary>
        /// Summary of key findings
        /// </summary>
        [JsonPropertyName("keyFindingsSummary")]
        [PromptProperty("Summary of key findings", true)]
        public string KeyFindingsSummary { get; set; }
        
        /// <summary>
        /// Notes on compliance
        /// </summary>
        [JsonPropertyName("regulatoryComplianceNotes")]
        [PromptProperty("Notes on compliance", true)]
        public List<string> RegulatoryComplianceNotes { get; set; }
        
        /// <summary>
        /// References to evidence points
        /// </summary>
        [JsonPropertyName("auditSupportingEvidenceReferences")]
        [PromptProperty("References to evidence points", true)]
        public List<string> AuditSupportingEvidenceReferences { get; set; }
    }
}
