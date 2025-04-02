using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Simplified consolidated audit report for Bouwdepot invoice validation
    /// </summary>
    public class ConsolidatedAuditReport
    {
        // Basic Invoice Information
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Vendor Information
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public VendorBankDetails BankDetails { get; set; } = new VendorBankDetails();
        
        // Overall Assessment (Summary)
        public bool IsApproved { get; set; }
        public int OverallScore { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        
        // Validation Results (Simplified)
        public DocumentValidation DocumentValidation { get; set; } = new DocumentValidation();
        public BouwdepotEligibility BouwdepotEligibility { get; set; } = new BouwdepotEligibility();
        public List<ValidatedLineItem> LineItems { get; set; } = new List<ValidatedLineItem>();
        public List<ValidatedLineItem> ApprovedItems { get; set; } = new List<ValidatedLineItem>();
        public List<ValidatedLineItem> ReviewItems { get; set; } = new List<ValidatedLineItem>();
        
        // Detailed Analysis Sections
        public List<DetailedAnalysisSection> DetailedAnalysis { get; set; } = new List<DetailedAnalysisSection>();
        
        // Audit Trail (Compliant with European law)
        public AuditTrail AuditInformation { get; set; } = new AuditTrail();
        
        // Optional Technical Details (expandable)
        public TechnicalDetails TechnicalDetails { get; set; } = new TechnicalDetails();
    }

    /// <summary>
    /// Vendor bank account details for payment processing
    /// </summary>
    public class VendorBankDetails
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;
        public string BIC { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
    }

    /// <summary>
    /// Document validation results in a simplified format
    /// </summary>
    public class DocumentValidation
    {
        public bool IsValid { get; set; }
        public int Score { get; set; }
        public string PrimaryReason { get; set; } = string.Empty;
        public List<string> ValidationDetails { get; set; } = new List<string>();
    }

    /// <summary>
    /// Bouwdepot eligibility assessment in a simplified format
    /// </summary>
    public class BouwdepotEligibility
    {
        public bool IsEligible { get; set; }
        public int Score { get; set; }
        public bool MeetsQualityImprovement { get; set; }
        public bool MeetsPermanentAttachment { get; set; }
        public string ConstructionType { get; set; } = string.Empty;
        public string PaymentPriority { get; set; } = string.Empty;
        public string SpecialConditions { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a validated line item from an invoice with simplified structure
    /// </summary>
    public class ValidatedLineItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsApproved { get; set; }
        public string ValidationNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Court-admissible audit trail that complies with European legal requirements
    /// </summary>
    public class AuditTrail
    {
        // Core audit information
        public string AuditIdentifier { get; set; } = Guid.NewGuid().ToString();
        public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;
        public string ValidationSystemVersion { get; set; } = string.Empty;
        
        // User and system information
        public string ValidatedBy { get; set; } = string.Empty;
        public string ValidatorIdentifier { get; set; } = string.Empty;
        public string ValidatorRole { get; set; } = string.Empty;
        public string SystemIPAddress { get; set; } = string.Empty;
        
        // Document integrity
        public string OriginalDocumentHash { get; set; } = string.Empty;
        public string DocumentSourceIdentifier { get; set; } = string.Empty;
        
        // Decision and reasoning
        public string ConclusionExplanation { get; set; } = string.Empty;
        public List<string> KeyFactors { get; set; } = new List<string>();
        
        // Legal references and compliance
        public List<RuleApplication> AppliedRules { get; set; } = new List<RuleApplication>();
        public List<LegalReference> LegalBasis { get; set; } = new List<LegalReference>();
        
        // Chain of custody
        public List<ProcessingStep> ProcessingSteps { get; set; } = new List<ProcessingStep>();
        
        // Digital signature
        public string AuditSignature { get; set; } = string.Empty;
        public string SignatureCertificateThumbprint { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reference to a specific legal or regulatory basis for a decision
    /// </summary>
    public class LegalReference
    {
        public string ReferenceCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Applicability { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a step in the processing chain with timing information
    /// </summary>
    public class ProcessingStep
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessorIdentifier { get; set; } = string.Empty;
        public string InputState { get; set; } = string.Empty;
        public string OutputState { get; set; } = string.Empty;
    }

    /// <summary>
    /// Documents the application of a specific rule to an invoice
    /// </summary>
    public class RuleApplication
    {
        public string RuleId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string RuleVersion { get; set; } = string.Empty;
        public string RuleDescription { get; set; } = string.Empty;
        public bool IsSatisfied { get; set; }
        public string ApplicationResult { get; set; } = string.Empty;
        public string EvidenceReference { get; set; } = string.Empty;
    }

    /// <summary>
    /// Technical details for advanced users or debugging
    /// </summary>
    public class TechnicalDetails
    {
        public Dictionary<string, string> DetailedMetrics { get; set; } = new Dictionary<string, string>();
        public List<string> ProcessingNotes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Detailed analysis section with name and key-value pairs
    /// </summary>
    public class DetailedAnalysisSection
    {
        public string SectionName { get; set; } = string.Empty;
        public Dictionary<string, string> AnalysisItems { get; set; } = new Dictionary<string, string>();
    }
}
