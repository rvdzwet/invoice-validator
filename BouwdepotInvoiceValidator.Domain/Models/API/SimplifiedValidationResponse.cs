using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Domain.Models.API
{
    /// <summary>
    /// Simplified response model for invoice validation API
    /// </summary>
    public class SimplifiedValidationResponse
    {
        /// <summary>
        /// Unique identifier for this validation
        /// </summary>
        public string ValidationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp when the validation was performed
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Overall validation status
        /// </summary>
        public ValidationStatus Status { get; set; } = ValidationStatus.Unknown;

        /// <summary>
        /// Summary of the validation result
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Extracted invoice data
        /// </summary>
        public InvoiceData Invoice { get; set; } = new InvoiceData();

        /// <summary>
        /// List of validation issues
        /// </summary>
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();

        /// <summary>
        /// List of fraud indicators
        /// </summary>
        public List<FraudIndicator> FraudIndicators { get; set; } = new List<FraudIndicator>();

        /// <summary>
        /// Fraud risk assessment
        /// </summary>
        public FraudRiskAssessment FraudRisk { get; set; } = new FraudRiskAssessment();

        /// <summary>
        /// Bouwdepot eligibility assessment
        /// </summary>
        public BouwdepotEligibility BouwdepotEligibility { get; set; } = new BouwdepotEligibility();

        /// <summary>
        /// Processing information
        /// </summary>
        public ProcessingInfo Processing { get; set; } = new ProcessingInfo();
    }

    /// <summary>
    /// Overall validation status
    /// </summary>
    public enum ValidationStatus
    {
        /// <summary>
        /// Validation has not been performed yet
        /// </summary>
        Unknown,

        /// <summary>
        /// The invoice is valid
        /// </summary>
        Valid,

        /// <summary>
        /// The invoice is invalid
        /// </summary>
        Invalid,

        /// <summary>
        /// The invoice requires manual review
        /// </summary>
        NeedsReview,

        /// <summary>
        /// An error occurred during validation
        /// </summary>
        Error
    }

    /// <summary>
    /// Extracted invoice data
    /// </summary>
    public class InvoiceData
    {
        /// <summary>
        /// Invoice number
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Due date for payment
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Total amount of the invoice
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// VAT/tax amount
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Currency code (e.g., "EUR", "USD")
        /// </summary>
        public string Currency { get; set; } = "EUR";

        /// <summary>
        /// Name of the vendor/supplier
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Address of the vendor/supplier
        /// </summary>
        public string VendorAddress { get; set; }

        /// <summary>
        /// KvK (Chamber of Commerce) number of the vendor
        /// </summary>
        public string VendorKvkNumber { get; set; }

        /// <summary>
        /// BTW (VAT) number of the vendor
        /// </summary>
        public string VendorBtwNumber { get; set; }

        /// <summary>
        /// Payment details
        /// </summary>
        public PaymentInfo Payment { get; set; } = new PaymentInfo();

        /// <summary>
        /// Line items in the invoice
        /// </summary>
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
    }

    /// <summary>
    /// Payment information
    /// </summary>
    public class PaymentInfo
    {
        /// <summary>
        /// The name of the account holder
        /// </summary>
        public string AccountHolderName { get; set; }

        /// <summary>
        /// International Bank Account Number
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// Bank Identifier Code (SWIFT code)
        /// </summary>
        public string BIC { get; set; }

        /// <summary>
        /// Bank name
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Payment reference or invoice number to include with payment
        /// </summary>
        public string PaymentReference { get; set; }

        /// <summary>
        /// Whether the payment details have been verified
        /// </summary>
        public bool IsVerified { get; set; }
    }

    /// <summary>
    /// Line item in an invoice
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantity of the item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Price per unit
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price for this line item
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// VAT/tax rate applied to this item
        /// </summary>
        public decimal VatRate { get; set; }

        /// <summary>
        /// Whether this item is eligible for Bouwdepot
        /// </summary>
        public bool IsBouwdepotEligible { get; set; }

        /// <summary>
        /// Reason why this item is or is not eligible for Bouwdepot
        /// </summary>
        public string EligibilityReason { get; set; }
    }

    /// <summary>
    /// Validation issue
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Type of issue
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Description of the issue
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity of the issue
        /// </summary>
        public IssueSeverity Severity { get; set; }

        /// <summary>
        /// Field or section that the issue relates to
        /// </summary>
        public string Field { get; set; }
    }

    /// <summary>
    /// Severity of a validation issue
    /// </summary>
    public enum IssueSeverity
    {
        /// <summary>
        /// Informational message
        /// </summary>
        Info,

        /// <summary>
        /// Warning message
        /// </summary>
        Warning,

        /// <summary>
        /// Error message
        /// </summary>
        Error,

        /// <summary>
        /// Critical error
        /// </summary>
        Critical
    }

    /// <summary>
    /// Fraud indicator
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Category of the fraud indicator
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Description of the indicator
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Confidence score for this indicator (0.0-1.0)
        /// </summary>
        public double? Confidence { get; set; }
    }

    /// <summary>
    /// Fraud risk assessment
    /// </summary>
    public class FraudRiskAssessment
    {
        /// <summary>
        /// Risk level assessment
        /// </summary>
        public FraudRiskLevel RiskLevel { get; set; } = FraudRiskLevel.Unknown;

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public int? RiskScore { get; set; }

        /// <summary>
        /// Summary of findings
        /// </summary>
        public string Summary { get; set; }
    }

    /// <summary>
    /// Fraud risk level
    /// </summary>
    public enum FraudRiskLevel
    {
        /// <summary>
        /// Risk level has not been assessed
        /// </summary>
        Unknown,

        /// <summary>
        /// Low risk of fraud
        /// </summary>
        Low,

        /// <summary>
        /// Medium risk of fraud
        /// </summary>
        Medium,

        /// <summary>
        /// High risk of fraud
        /// </summary>
        High,

        /// <summary>
        /// Very high risk of fraud
        /// </summary>
        VeryHigh
    }

    /// <summary>
    /// Bouwdepot eligibility assessment
    /// </summary>
    public class BouwdepotEligibility
    {
        /// <summary>
        /// Whether the invoice is eligible for Bouwdepot
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Reason why the invoice is or is not eligible for Bouwdepot
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Percentage of the invoice that is eligible for Bouwdepot (0-100)
        /// </summary>
        public decimal EligiblePercentage { get; set; }

        /// <summary>
        /// Amount of the invoice that is eligible for Bouwdepot
        /// </summary>
        public decimal EligibleAmount { get; set; }
    }

    /// <summary>
    /// Processing information
    /// </summary>
    public class ProcessingInfo
    {
        /// <summary>
        /// Duration of the processing in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// AI models used during processing
        /// </summary>
        public List<string> ModelsUsed { get; set; } = new List<string>();

        /// <summary>
        /// Processing steps performed
        /// </summary>
        public List<string> Steps { get; set; } = new List<string>();
    }
}
