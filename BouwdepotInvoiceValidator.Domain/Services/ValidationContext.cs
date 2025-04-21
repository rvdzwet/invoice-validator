using BouwdepotValidationValidator.Infrastructure.Abstractions;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Context for validation of construction fund withdrawal proof documents
    /// </summary>
    public class ValidationContext
    {
        /// <summary>
        /// Unique identifier for this validation context
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Information about the input document
        /// </summary>
        public InputDocumentInfo InputDocument { get; set; }






        /// <summary>
        /// Comprehensive validation response with detailed analysis
        /// </summary>
        public ComprehensiveWithdrawalProofResponse ComprehensiveValidationResult { get; set; }

        /// <summary>
        /// Overall outcome of the validation
        /// </summary>
        public ValidationOutcome OverallOutcome { get; set; } = ValidationOutcome.Unknown;

        /// <summary>
        /// Summary of the overall outcome
        /// </summary>
        public string OverallOutcomeSummary { get; set; } = "Validation has not been completed.";

        /// <summary>
        /// List of processing steps
        /// </summary>
        public List<ProcessingStepLog> ProcessingSteps { get; } = new List<ProcessingStepLog>();

        /// <summary>
        /// List of issues encountered during validation
        /// </summary>
        public List<ProcessingIssue> Issues { get; } = new List<ProcessingIssue>();

        /// <summary>
        /// List of AI models used during validation
        /// </summary>
        public List<AIModelUsage> AIModelsUsed { get; } = new List<AIModelUsage>();

        /// <summary>
        /// List of validation results
        /// </summary>
        public List<RuleValidationResult> ValidationResults { get; } = new List<RuleValidationResult>();
        

        
        /// <summary>
        /// Conversation context for maintaining history between LLM calls
        /// </summary>
        public ConversationContext ConversationContext { get; set; }
        public TimeSpan ElapsedTime { get; internal set; }

        /// <summary>
        /// Adds a processing step to the context
        /// </summary>
        public void AddProcessingStep(string stepName, string description, ProcessingStepStatus status)
        {
            ProcessingSteps.Add(new ProcessingStepLog
            {
                StepName = stepName,
                Description = description,
                Status = status,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        /// <summary>
        /// Adds an issue to the context
        /// </summary>
        public void AddIssue(string issueType, string description, IssueSeverity severity, string field = null)
        {
            Issues.Add(new ProcessingIssue
            {
                IssueType = issueType,
                Description = description,
                Severity = severity,
                Field = field,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        /// <summary>
        /// Adds an AI model usage to the context
        /// </summary>
        public void AddAIModelUsage(string modelName, string modelVersion, string operation, int tokenCount)
        {
            AIModelsUsed.Add(new AIModelUsage
            {
                ModelName = modelName,
                ModelVersion = modelVersion,
                Operation = operation,
                TokenCount = tokenCount,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
    }

    /// <summary>
    /// Invoice data extracted from a document
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Original filename
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Invoice number
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// VAT amount
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        public string Currency { get; set; } = "EUR";

        /// <summary>
        /// Vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Vendor address
        /// </summary>
        public string VendorAddress { get; set; }

        /// <summary>
        /// Vendor KvK number
        /// </summary>
        public string VendorKvkNumber { get; set; }

        /// <summary>
        /// Vendor BTW number
        /// </summary>
        public string VendorBtwNumber { get; set; }

        /// <summary>
        /// Payment details
        /// </summary>
        public PaymentDetails PaymentDetails { get; set; } = new PaymentDetails();

        /// <summary>
        /// Line items
        /// </summary>
        public List<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

        /// <summary>
        /// Raw text extracted from the invoice
        /// </summary>
        public string RawText { get; set; }
    }

    /// <summary>
    /// Payment details
    /// </summary>
    public class PaymentDetails
    {
        /// <summary>
        /// Account holder name
        /// </summary>
        public string AccountHolderName { get; set; }

        /// <summary>
        /// IBAN
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// BIC
        /// </summary>
        public string BIC { get; set; }

        /// <summary>
        /// Bank name
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Payment reference
        /// </summary>
        public string PaymentReference { get; set; }
    }

    /// <summary>
    /// Line item in an invoice
    /// </summary>
    public class InvoiceLineItem
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// VAT rate
        /// </summary>
        public decimal VatRate { get; set; }

        /// <summary>
        /// Whether the item is eligible for Bouwdepot
        /// </summary>
        public bool IsBouwdepotEligible { get; set; }

        /// <summary>
        /// Reason for eligibility decision
        /// </summary>
        public string EligibilityReason { get; set; }
    }

    /// <summary>
    /// Processing issue
    /// </summary>
    public class ProcessingIssue
    {
        /// <summary>
        /// Type of issue
        /// </summary>
        public string IssueType { get; set; }

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

        /// <summary>
        /// Timestamp when the issue was recorded
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Stack trace for errors
        /// </summary>
        public string StackTrace { get; set; }
    }
}
