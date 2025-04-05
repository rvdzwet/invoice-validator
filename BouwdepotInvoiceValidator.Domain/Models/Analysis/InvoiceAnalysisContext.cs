using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BouwdepotInvoiceValidator.Domain.Models;

namespace BouwdepotInvoiceValidator.Domain.Models.Analysis
{
    /// <summary>
    /// Represents the context for invoice analysis, containing all data and state throughout the analysis process
    /// </summary>
    public class InvoiceAnalysisContext
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

    /// <summary>
    /// Information about the input document
    /// </summary>
    public class InputDocumentInfo
    {
        /// <summary>
        /// Original file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// MIME type of the document
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Number of pages in the document
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// MD5 hash of the file content
        /// </summary>
        public string ContentHash { get; set; }

        /// <summary>
        /// Timestamp when the document was uploaded
        /// </summary>
        public DateTimeOffset UploadTimestamp { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Log entry for a processing step
    /// </summary>
    public class ProcessingStepLog
    {
        /// <summary>
        /// Name of the processing step
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Description of the step (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status of the step
        /// </summary>
        public ProcessingStepStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the step was performed
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Duration of the step in milliseconds
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Additional details about the step
        /// </summary>
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Status of a processing step
    /// </summary>
    public enum ProcessingStepStatus
    {
        /// <summary>
        /// Step is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Step completed successfully
        /// </summary>
        Success,

        /// <summary>
        /// Step completed with warnings
        /// </summary>
        Warning,

        /// <summary>
        /// Step failed
        /// </summary>
        Error,

        /// <summary>
        /// Step was skipped
        /// </summary>
        Skipped
    }

    /// <summary>
    /// Log entry for an AI model usage
    /// </summary>
    public class AIModelUsage
    {
        /// <summary>
        /// Name of the AI model
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Version of the AI model
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Operation performed with the model
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Number of tokens used
        /// </summary>
        public int TokenCount { get; set; }

        /// <summary>
        /// Timestamp when the model was used
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Log entry for a processing issue
    /// </summary>
    public class ProcessingIssue
    {
        /// <summary>
        /// Type of issue
        /// </summary>
        public string IssueType { get; set; }

        /// <summary>
        /// Description of the issue (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity of the issue
        /// </summary>
        public IssueSeverity Severity { get; set; }

        /// <summary>
        /// Timestamp when the issue occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Stack trace if available
        /// </summary>
        public string StackTrace { get; set; }
    }

    /// <summary>
    /// Severity of a processing issue
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
    /// Overall outcome of the validation
    /// </summary>
    public enum ValidationOutcome
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
    /// Result of applying a validation rule
    /// </summary>
    public class RuleValidationResult
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Description of the rule (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Result of the rule validation
        /// </summary>
        public RuleResult Result { get; set; }

        /// <summary>
        /// Explanation for the result (in DetectedLanguage)
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// Severity of the rule
        /// </summary>
        public RuleSeverity Severity { get; set; }

        /// <summary>
        /// Additional details about the rule validation
        /// </summary>
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Result of a rule validation
    /// </summary>
    public enum RuleResult
    {
        /// <summary>
        /// The rule passed
        /// </summary>
        Pass,

        /// <summary>
        /// The rule failed
        /// </summary>
        Fail,

        /// <summary>
        /// The rule was not applicable
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The rule passed but with warnings
        /// </summary>
        Warning
    }

    /// <summary>
    /// Severity of a rule
    /// </summary>
    public enum RuleSeverity
    {
        /// <summary>
        /// Informational rule
        /// </summary>
        Info,

        /// <summary>
        /// Warning rule
        /// </summary>
        Warning,

        /// <summary>
        /// Error rule
        /// </summary>
        Error,

        /// <summary>
        /// Critical rule
        /// </summary>
        Critical
    }

    /// <summary>
    /// Result of fraud analysis
    /// </summary>
    public class FraudAnalysisResult
    {
        /// <summary>
        /// Risk level assessment
        /// </summary>
        public string RiskLevel { get; set; }

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public int? RiskScore { get; set; }

        /// <summary>
        /// Summary of findings (in DetectedLanguage)
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detected fraud indicators
        /// </summary>
        public List<FraudIndicator> Indicators { get; set; } = new List<FraudIndicator>();
    }

    /// <summary>
    /// A detected fraud indicator
    /// </summary>
    public class FraudIndicator
    {
        /// <summary>
        /// Category of the fraud indicator
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Description of the indicator (in DetectedLanguage)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Confidence score for this indicator (0.0-1.0)
        /// </summary>
        public double? Confidence { get; set; }

        /// <summary>
        /// Location of the indicator in the document, if applicable
        /// </summary>
        public RectangleF? Location { get; set; }
    }
}
