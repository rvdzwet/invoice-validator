using System.Drawing;

namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Represents the context for invoice analysis, containing all data and state throughout the analysis process
    /// </summary>
    public class ValidationContext
    {
        /// <summary>
        /// Unique identifier for this analysis context
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp when the analysis was started
        /// </summary>
        public DateTimeOffset StartTimestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Information about the input document
        /// </summary>
        public InputDocumentInfo InputDocument { get; set; } = new InputDocumentInfo();

        /// <summary>
        /// The primary language detected in the invoice. ISO 639-1 code (e.g., "nl", "en")
        /// </summary>
        public string DetectedLanguage { get; set; } = "nl";

        /// <summary>
        /// Confidence score for the language detection (0.0 to 1.0)
        /// </summary>
        public double? LanguageDetectionConfidence { get; set; }

        /// <summary>
        /// The extracted invoice data
        /// </summary>
        public Invoice ExtractedInvoice { get; set; }

        /// <summary>
        /// Results of fraud analysis
        /// </summary>
        public FraudAnalysisResult FraudAnalysis { get; set; }

        /// <summary>
        /// Results of applying validation rules
        /// </summary>
        public List<RuleValidationResult> ValidationResults { get; set; } = new List<RuleValidationResult>();

        /// <summary>
        /// Overall outcome of the validation
        /// </summary>
        public ValidationOutcome OverallOutcome { get; set; } = ValidationOutcome.Unknown;

        /// <summary>
        /// Summary explanation for the overall outcome (in DetectedLanguage)
        /// </summary>
        public string OverallOutcomeSummary { get; set; }

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
        /// Adds a processing step to the log
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
        /// Adds an AI model usage to the log
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

        /// <summary>
        /// Adds an issue to the log
        /// </summary>
        public void AddIssue(string issueType, string description, IssueSeverity severity)
        {
            Issues.Add(new ProcessingIssue
            {
                IssueType = issueType,
                Description = description,
                Severity = severity,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
    }
}
