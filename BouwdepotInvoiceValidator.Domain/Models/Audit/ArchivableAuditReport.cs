using BouwdepotInvoiceValidator.Domain.Models.Analysis;

namespace BouwdepotInvoiceValidator.Domain.Models.Audit
{
    /// <summary>
    /// Represents an audit report that can be persisted to storage
    /// </summary>
    public class ArchivableAuditReport
    {
        /// <summary>
        /// Unique identifier for this audit report
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp when the audit report was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// ID of the invoice analysis context that this audit report is based on
        /// </summary>
        public string AnalysisContextId { get; set; }

        /// <summary>
        /// Original file name of the invoice
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Overall validation outcome
        /// </summary>
        public ValidationOutcome Outcome { get; set; }

        /// <summary>
        /// Summary explanation for the overall outcome
        /// </summary>
        public string OutcomeSummary { get; set; }

        /// <summary>
        /// Extracted invoice data
        /// </summary>
        public Invoice Invoice { get; set; }

        /// <summary>
        /// Results of fraud analysis
        /// </summary>
        public FraudAnalysisResult FraudAnalysis { get; set; }

        /// <summary>
        /// Results of applying validation rules
        /// </summary>
        public List<RuleValidationResult> ValidationResults { get; set; } = new List<RuleValidationResult>();

        /// <summary>
        /// Log of the processing steps performed
        /// </summary>
        public List<ProcessingStepLog> ProcessingSteps { get; set; } = new List<ProcessingStepLog>();

        /// <summary>
        /// Log of AI models used during the analysis
        /// </summary>
        public List<AIModelUsage> AIModelsUsed { get; set; } = new List<AIModelUsage>();

        /// <summary>
        /// Log of any issues encountered during processing
        /// </summary>
        public List<ProcessingIssue> Issues { get; set; } = new List<ProcessingIssue>();

        /// <summary>
        /// Creates an audit report from an invoice analysis context
        /// </summary>
        /// <param name="context">The invoice analysis context</param>
        /// <returns>The created audit report</returns>
        public static ArchivableAuditReport FromAnalysisContext(InvoiceAnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new ArchivableAuditReport
            {
                AnalysisContextId = context.Id.ToString(),
                FileName = context.InputDocument?.FileName,
                Outcome = context.OverallOutcome,
                OutcomeSummary = context.OverallOutcomeSummary,
                Invoice = context.ExtractedInvoice,
                FraudAnalysis = context.FraudAnalysis,
                ValidationResults = new List<RuleValidationResult>(context.ValidationResults),
                ProcessingSteps = new List<ProcessingStepLog>(context.ProcessingSteps),
                AIModelsUsed = new List<AIModelUsage>(context.AIModelsUsed),
                Issues = new List<ProcessingIssue>(context.Issues)
            };
        }
    }
}
